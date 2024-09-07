using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIM
{
    internal class CeilingFunc
    {
        // modification
        public static void Modify(Ceiling ceiling, string property_name, string property_value)
        {
            switch (property_name)
            {
                case "Level":
                    ModifyLevel(ceiling, property_value);
                    break;
                case "Height Offset From Level":
                    ModifyHeightOffsetFromLevel(ceiling, double.Parse(property_value));
                    break;
                case "Room Bounding":
                    ModifyRoomBounding(ceiling, int.Parse(property_value));
                    break;
                case "Slope":
                    ModifySlope(ceiling, double.Parse(property_value));
                    break;
                case "Comments":
                    ModifyComments(ceiling, property_value);
                    break;
                case "Material":
                    ModifyMaterial(ceiling, property_value);
                    break;
                case "Color":
                    ModifyColor(ceiling, property_value);
                    break;
                case "Thickness":
                    ModifyThickness(ceiling, double.Parse(property_value));
                    break;
                case "Type Comments":
                    ModifyTypeComments(ceiling, property_value);
                    break;
            }
        }

        // instance properties
        public static void ModifyLevel(Ceiling ceiling, string level_str)
        {
            // find the level id with the given name
            Level level = new FilteredElementCollector(ceiling.Document)
                .OfClass(typeof(Level))
                .Where(x => x.Name == level_str)
                .Cast<Level>()
                .FirstOrDefault();
            // modify the level of the ceiling
            using (Transaction tr = new Transaction(ceiling.Document, "modify_ceiling_level"))
            {
                tr.Start();
                ceiling.get_Parameter(BuiltInParameter.LEVEL_PARAM).Set(level.Id);
                tr.Commit();
            }
        }

        public static void ModifyHeightOffsetFromLevel(Ceiling ceiling, double height)
        {
            // modify the base offset
            // the metric of offest here is feet
            using (Transaction tr = new Transaction(ceiling.Document, "modify_ceiling_height_offset"))
            {
                tr.Start();
                ceiling.get_Parameter(BuiltInParameter.CEILING_HEIGHTABOVELEVEL_PARAM).Set(height);
                tr.Commit();
            }
        }

        public static void ModifyRoomBounding(Ceiling ceiling, int rb)
        {
            // modify the value of room bounding property
            // 0->no, 1->yes
            using (Transaction tr = new Transaction(ceiling.Document, "modify_ceiling_room_bounding"))
            {
                tr.Start();
                ceiling.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(rb);
                tr.Commit();
            }
        }

        public static void ModifySlope(Ceiling ceiling, double slope)
        {
            // modify the value of slope
            // the metirc here is the value of tan(degree), where degree is shown in the Revit UI
            using (Transaction tr = new Transaction(ceiling.Document, "modify_ceiling_slope"))
            {
                tr.Start();
                ceiling.get_Parameter(BuiltInParameter.ROOF_SLOPE).Set(slope);
                tr.Commit();
            }
        }

        public static void ModifyComments(Ceiling ceiling, string comment)
        {
            // modify the comments of the ceiling
            using (Transaction tr = new Transaction(ceiling.Document, "modify_ceiling_comments"))
            {
                tr.Start();
                ceiling.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set(comment);
                tr.Commit();
            }
        }

        // type properties
        public static void ModifyMaterial(Ceiling ceiling, string material_name)
        {
            // modify the material of the ceiling type
            // duplicate the ceiling type of the ceiling
            CeilingType ceilingType = ceiling.Document.GetElement(ceiling.GetTypeId()) as CeilingType;
            string origin_name = ceilingType.Name;
            // get all the mateirals
            FilteredElementCollector collector = new FilteredElementCollector(ceiling.Document);
            collector.OfClass(typeof(Material));

            List<string> materialNames = new List<string>();
            List<ElementId> materialIds = new List<ElementId>();

            foreach (Material material in collector)
            {
                materialNames.Add(material.Name);
                materialIds.Add(material.Id);
            }

            // find the material id with the similarites with the given name using Levenshtein distance
            float min_distance = 10000;
            int min_index = -1;
            for (int i = 0; i < materialNames.Count; i++)
            {
                float distance = Utils.LevenshteinDistance(material_name, materialNames[i]);
                if (distance < min_distance)
                {
                    min_distance = distance;
                    min_index = i;
                }
            }
            if (min_index == -1)
            {
                return;
            }
            // modify the property of the new type
            using (Transaction tr = new Transaction(ceiling.Document, "modify_ceiling_material"))
            {
                tr.Start();
                ElementId new_type_id = ceilingType.Duplicate(origin_name + "_material_" + material_name).Id;
                CeilingType newCeilingType = ceiling.Document.GetElement(new_type_id) as CeilingType;
                CompoundStructure cs = newCeilingType.GetCompoundStructure();
                IList<CompoundStructureLayer> layers = cs.GetLayers();
                layers[0].MaterialId = materialIds[min_index];
                cs.SetLayers(layers);
                newCeilingType.SetCompoundStructure(cs);
                ceiling.ChangeTypeId(new_type_id);
                tr.Commit();
            }
        }

        public static void ModifyColor(Ceiling ceiling, string color)
        {
            // mapping from string to Revit Color
            Dictionary<string, Color> color_map = new Dictionary<string, Color>();
            color_map.Add("Red", new Color(255, 0, 0));
            color_map.Add("Green", new Color(0, 255, 0));
            color_map.Add("Blue", new Color(0, 0, 255));
            color_map.Add("Yellow", new Color(255, 255, 0));
            color_map.Add("Orange", new Color(255, 128, 0));
            color_map.Add("Purple", new Color(128, 0, 255));
            color_map.Add("Pink", new Color(255, 0, 255));
            color_map.Add("Brown", new Color(128, 64, 0));
            color_map.Add("Gray", new Color(128, 128, 128));
            color_map.Add("White", new Color(255, 255, 255));
            color_map.Add("Black", new Color(0, 0, 0));
            // duplicate the ceiling type
            CeilingType ceilingType = ceiling.Document.GetElement(ceiling.GetTypeId()) as CeilingType;
            string origin_name = ceilingType.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(ceiling.Document, "modify_ceiling_color"))
            {
                tr.Start();
                ElementId new_type_id = ceilingType.Duplicate(origin_name + "_color_" + color).Id;
                CeilingType newCeilingType = ceiling.Document.GetElement(new_type_id) as CeilingType;
                CompoundStructure cs = newCeilingType.GetCompoundStructure();
                IList<CompoundStructureLayer> layers = cs.GetLayers();
                Material material = ceiling.Document.GetElement(layers[0].MaterialId) as Material;
                material = material.Duplicate(material.Name + "_color_" + color);
                material.Color = color_map[color];
                layers[0].MaterialId = material.Id;
                cs.SetLayers(layers);
                newCeilingType.SetCompoundStructure(cs);
                ceiling.ChangeTypeId(new_type_id);
                tr.Commit();
            }
        }

        public static void ModifyThickness(Ceiling ceiling, double thickness)
        {
            // modify the thickness
            // the metric here is feet
            // duplicate the type
            CeilingType ceilingType = ceiling.Document.GetElement(ceiling.GetTypeId()) as CeilingType;
            string origin_name = ceilingType.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(ceiling.Document, "modify_ceiling_thickness"))
            {
                tr.Start();
                ElementId new_type_id = ceilingType.Duplicate(origin_name + "_thickness_" + thickness.ToString()).Id;
                CeilingType newCeilingType = ceiling.Document.GetElement(new_type_id) as CeilingType;
                CompoundStructure cs = newCeilingType.GetCompoundStructure();
                IList<CompoundStructureLayer> layers = cs.GetLayers();
                layers[0].Width = thickness;
                cs.SetLayers(layers);
                newCeilingType.SetCompoundStructure(cs);
                ceiling.ChangeTypeId(new_type_id);
                tr.Commit();
            }
        }

        public static void ModifyTypeComments(Ceiling ceiling, string comment)
        {
            // modify the comments of the ceiling type
            CeilingType ceilingType = ceiling.Document.GetElement(ceiling.GetTypeId()) as CeilingType;
            string origin_name = ceilingType.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(ceiling.Document, "modify_ceiling_type_comments"))
            {
                tr.Start();
                ElementId new_type_id = ceilingType.Duplicate(origin_name + "_comments_" + comment).Id;
                CeilingType newCeilingType = ceiling.Document.GetElement(new_type_id) as CeilingType;
                newCeilingType.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS).Set(comment);
                ceiling.ChangeTypeId(new_type_id);
                tr.Commit();
            }
        }

        // retrieval
        public static void Retrieval(Ceiling ceiling, string option)
        {
            // retrieve corresponding properties of the ceiling and display in the Revit UI
            switch (option)
            {
                case "Level":
                    TaskDialog.Show("Level", ceiling.get_Parameter(BuiltInParameter.LEVEL_PARAM).AsValueString());
                    break;
                case "Height Offset From Level":
                    TaskDialog.Show("Height Offset From Level", ceiling.get_Parameter(BuiltInParameter.CEILING_HEIGHTABOVELEVEL_PARAM).AsValueString());
                    break;
                case "Room Bounding":
                    TaskDialog.Show("Room Bounding", ceiling.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).AsValueString());
                    break;
                case "Slope":
                    TaskDialog.Show("Slope", ceiling.get_Parameter(BuiltInParameter.ROOF_SLOPE).AsValueString());
                    break;
                case "Comments":
                    TaskDialog.Show("Comments", ceiling.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsValueString());
                    break;
                case "Material":
                    CeilingType ceilingType = ceiling.Document.GetElement(ceiling.GetTypeId()) as CeilingType;
                    CompoundStructure cs = ceilingType.GetCompoundStructure();
                    IList<CompoundStructureLayer> layers = cs.GetLayers();
                    Material material = ceiling.Document.GetElement(layers[0].MaterialId) as Material;
                    TaskDialog.Show("Material", material.Name);
                    break;
                case "Color":
                    CeilingType ceilingType1 = ceiling.Document.GetElement(ceiling.GetTypeId()) as CeilingType;
                    CompoundStructure cs1 = ceilingType1.GetCompoundStructure();
                    IList<CompoundStructureLayer> layers1 = cs1.GetLayers();
                    Material material1 = ceiling.Document.GetElement(layers1[0].MaterialId) as Material;
                    TaskDialog.Show("Color (RGB)", material1.Color.Red.ToString() + " " + material1.Color.Green.ToString() + " " + material1.Color.Blue.ToString());
                    break;
                case "Thickness":
                    CeilingType ceilingType2 = ceiling.Document.GetElement(ceiling.GetTypeId()) as CeilingType;
                    CompoundStructure cs2 = ceilingType2.GetCompoundStructure();
                    IList<CompoundStructureLayer> layers2 = cs2.GetLayers();
                    TaskDialog.Show("Thickness", layers2[0].Width.ToString());
                    break;
                case "Type Comments":
                    CeilingType ceilingType3 = ceiling.Document.GetElement(ceiling.GetTypeId()) as CeilingType;
                    TaskDialog.Show("Type Comments", ceilingType3.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS).AsValueString());
                    break;
                case "Perimeter":
                    TaskDialog.Show("Perimeter", ceiling.get_Parameter(BuiltInParameter.HOST_PERIMETER_COMPUTED).AsValueString());
                    break;
                case "Area":
                    TaskDialog.Show("Area", ceiling.get_Parameter(BuiltInParameter.HOST_AREA_COMPUTED).AsValueString());
                    break;
                case "Volumn":
                    TaskDialog.Show("Volumn", ceiling.get_Parameter(BuiltInParameter.HOST_VOLUME_COMPUTED).AsValueString());
                    break;
                case "Coordinate":
                    BoundingBoxXYZ bounding = ceiling.get_BoundingBox(ceiling.Document.ActiveView);
                    TaskDialog.Show("Coordinate", "Max: " + bounding.Max.ToString() + "\nMin: " + bounding.Min.ToString());
                    break;
            }
        }

        // deletion
        public static void Delete(Ceiling ceiling)
        {
            using (Transaction tr = new Transaction(ceiling.Document, "delete_ceiling"))
            {
                tr.Start();
                ceiling.Document.Delete(ceiling.Id);
                tr.Commit();
            }
        }

        // creation
        public static void Create(Document doc, XYZ p1, XYZ p2, XYZ p3, XYZ p4, Dictionary<string, string> dic)
        {
            Curve line_1 = Line.CreateBound(p1, p2);
            Curve line_2 = Line.CreateBound(p2, p3);
            Curve line_3 = Line.CreateBound(p3, p4);
            Curve line_4 = Line.CreateBound(p4, p1);

            List<Curve> curves = new List<Curve>() { line_1, line_2, line_3, line_4 };
            CurveLoop curveLoop = CurveLoop.Create(curves);
            IList<CurveLoop> curveLoop_l = new List<CurveLoop>() { curveLoop };

            Level level = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .First() as Level;
            if (dic.ContainsKey("Level"))
            {
                string level_name = dic["Level"];
                level = new FilteredElementCollector(doc)
                    .OfClass(typeof(Level))
                    .Where(x => x.Name == level_name)
                    .Cast<Level>()
                    .FirstOrDefault();
            }

            CeilingType ceilingType = new FilteredElementCollector(doc)
                .OfClass(typeof(CeilingType))
                .WhereElementIsElementType()
                .Cast<CeilingType>()
                .FirstOrDefault(x => x.FamilyName == "Compound Ceiling" && x.GetCompoundStructure().GetLayers().Count == 1);

            Ceiling ceiling = null;
            // create the ceiling
            using (Transaction tr = new Transaction(doc, "create_ceiling"))
            {
                tr.Start();
                ceiling = Ceiling.Create(doc, curveLoop_l, ceilingType.Id, level.Id);
                tr.Commit();
            }
            // modify based on the user arguments
            if (dic.ContainsKey("Height Offset From Level"))
            {
                ModifyHeightOffsetFromLevel(ceiling, double.Parse(dic["Height Offset From Level"]));
            }
            if (dic.ContainsKey("Room Bounding"))
            {
                ModifyRoomBounding(ceiling, int.Parse(dic["Room Bounding"]));
            }
            if (dic.ContainsKey("Slope"))
            {
                ModifySlope(ceiling, double.Parse(dic["Slope"]));
            }
            if (dic.ContainsKey("Comments"))
            {
                ModifyComments(ceiling, dic["Comments"]);
            }
            if (dic.ContainsKey("Material"))
            {
                ModifyMaterial(ceiling, dic["Material"]);
            }
            if (dic.ContainsKey("Color"))
            {
                ModifyColor(ceiling, dic["Color"]);
            }
            if (dic.ContainsKey("Thickness"))
            {
                ModifyThickness(ceiling, double.Parse(dic["Thickness"]));
            }
            if (dic.ContainsKey("Type Comments"))
            {
                ModifyTypeComments(ceiling, dic["Type Comments"]);
            }
        }
    }
}
