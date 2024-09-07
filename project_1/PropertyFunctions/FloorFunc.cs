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
    internal class FloorFunc
    {
        // modification
        public static void Modify(Floor floor, string property_name, string property_value)
        {
            switch (property_name)
            {
                case "Level":
                    ModifyLevel(floor, property_value);
                    break;
                case "Height Offset From Level":
                    ModifyHeightOffsetFromLevel(floor, double.Parse(property_value));
                    break;
                case "Room Bounding":
                    ModifyRoomBounding(floor, int.Parse(property_value));
                    break;
                case "Structural":
                    ModifyStructural(floor, int.Parse(property_value));
                    break;
                case "Slope":
                    ModifySlope(floor, double.Parse(property_value));
                    break;
                case "Comments":
                    ModifyComments(floor, property_value);
                    break;
                case "Material":
                    ModifyMaterial(floor, property_value);
                    break;
                case "Color":
                    ModifyColor(floor, property_value);
                    break;
                case "Thickness":
                    ModifyThickness(floor, double.Parse(property_value));
                    break;
                case "Function":
                    ModifyFunction(floor, int.Parse(property_value));
                    break;
                case "Type Comments":
                    ModifyTypeComments(floor, property_value);
                    break;
            }
        }

        // instance properties
        public static void ModifyLevel(Floor floor, string level_str)
        {
            // find the level id with the given name
            Level level = new FilteredElementCollector(floor.Document)
                .OfClass(typeof(Level))
                .Where(x => x.Name == level_str)
                .Cast<Level>()
                .FirstOrDefault();
            // modify the level of the floor
            using (Transaction tr = new Transaction(floor.Document, "modify_floor_level"))
            {
                tr.Start();
                floor.get_Parameter(BuiltInParameter.LEVEL_PARAM).Set(level.Id);
                tr.Commit();
            }
        }

        public static void ModifyHeightOffsetFromLevel(Floor floor, double height)
        {
            // modify the base offset
            // the metric of offest here is feet
            using (Transaction tr = new Transaction(floor.Document, "modify_floor_height_offset"))
            {
                tr.Start();
                floor.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM).Set(height);
                tr.Commit();
            }
        }

        public static void ModifyRoomBounding(Floor floor, int rb)
        {
            // modify the value of room bounding property
            // 0->no, 1->yes
            using (Transaction tr = new Transaction(floor.Document, "modify_floor_room_bounding"))
            {
                tr.Start();
                floor.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(rb);
                tr.Commit();
            }
        }

        public static void ModifyStructural(Floor floor, int structural)
        {
            // modify the value of structural property
            // 0->no, 1->yes
            using (Transaction tr = new Transaction(floor.Document, "modify_floor_structural"))
            {
                tr.Start();
                floor.get_Parameter(BuiltInParameter.FLOOR_PARAM_IS_STRUCTURAL).Set(structural);
                tr.Commit();
            }
        }

        public static void ModifySlope(Floor floor, double slope)
        {
            // modify the value of slope
            // the metirc here is the value of tan(degree), where degree is shown in the Revit UI
            using (Transaction tr = new Transaction(floor.Document, "modify_floor_slope"))
            {
                tr.Start();
                floor.get_Parameter(BuiltInParameter.ROOF_SLOPE).Set(slope);
                tr.Commit();
            }
        }

        public static void ModifyComments(Floor floor, string comment)
        {
            // modify the comments of the floor
            using (Transaction tr = new Transaction(floor.Document, "modify_floor_comments"))
            {
                tr.Start();
                floor.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set(comment);
                tr.Commit();
            }
        }

        // type properties
        public static void ModifyMaterial(Floor floor, string material_name)
        {
            // modify the material of the floor type
            // duplicate the floor type of the floor
            FloorType floorType = floor.FloorType;
            string origin_name = floorType.Name;
            // get all the mateirals
            FilteredElementCollector collector = new FilteredElementCollector(floor.Document);
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
            using (Transaction tr = new Transaction(floor.Document, "modify_floor_material"))
            {
                tr.Start();
                ElementId new_type_id = floorType.Duplicate(origin_name + "_material_" + material_name).Id;
                FloorType newFloorType = floor.Document.GetElement(new_type_id) as FloorType;
                CompoundStructure cs = floor.FloorType.GetCompoundStructure();
                IList<CompoundStructureLayer> layers = cs.GetLayers();
                layers[0].MaterialId = materialIds[min_index];
                cs.SetLayers(layers);
                newFloorType.SetCompoundStructure(cs);
                floor.FloorType = floor.Document.GetElement(new_type_id) as FloorType;
                tr.Commit();
            }
        }

        public static void ModifyColor(Floor floor, string color)
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
            // duplicate the floor type
            FloorType floorType = floor.FloorType;
            string origin_name = floorType.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(floor.Document, "modify_floor_color"))
            {
                tr.Start();
                ElementId new_type_id = floorType.Duplicate(origin_name + "_color_" + color).Id;
                FloorType newFloorType = floor.Document.GetElement(new_type_id) as FloorType;
                CompoundStructure cs = floor.FloorType.GetCompoundStructure();
                IList<CompoundStructureLayer> layers = cs.GetLayers();
                Material material = floor.Document.GetElement(layers[0].MaterialId) as Material;
                material = material.Duplicate(material.Name + "_color_" + color);
                material.Color = color_map[color];
                layers[0].MaterialId = material.Id;
                cs.SetLayers(layers);
                newFloorType.SetCompoundStructure(cs);
                floor.FloorType = floor.Document.GetElement(new_type_id) as FloorType;
                tr.Commit();
            }
        }

        public static void ModifyThickness(Floor floor, double thickness)
        {
            // modify the thickness
            // the metric here is feet
            // duplicate the type
            FloorType floorType = floor.FloorType;
            string origin_name = floorType.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(floor.Document, "modify_floor_thickness"))
            {
                tr.Start();
                ElementId new_type_id = floorType.Duplicate(origin_name + "_thickness_" + thickness.ToString()).Id;
                FloorType newFloorType = floor.Document.GetElement(new_type_id) as FloorType;
                CompoundStructure cs = floor.FloorType.GetCompoundStructure();
                IList<CompoundStructureLayer> layers = cs.GetLayers();
                layers[0].Width = thickness;
                cs.SetLayers(layers);
                newFloorType.SetCompoundStructure(cs);
                floor.FloorType = floor.Document.GetElement(new_type_id) as FloorType;
                tr.Commit();
            }
        }

        public static void ModifyFunction(Floor floor, int fun)
        {
            // modify the function of the floor type
            // 0->Interior, 1->Exterior
            // duplicate the floor type of the floor
            FloorType floorType = floor.FloorType;
            string origin_name = floorType.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(floor.Document, "modify_floor_function"))
            {
                tr.Start();
                ElementId new_type_id = floorType.Duplicate(origin_name + "_function_" + fun.ToString()).Id;
                floor.Document.GetElement(new_type_id).get_Parameter(BuiltInParameter.FUNCTION_PARAM).Set(fun);
                floor.FloorType = floor.Document.GetElement(new_type_id) as FloorType;
                tr.Commit();
            }
        }

        public static void ModifyTypeComments(Floor floor, string comment)
        {
            // modify the comments of the floor type
            FloorType floorType = floor.FloorType;
            string origin_name = floorType.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(floor.Document, "modify_floor_type_comments"))
            {
                tr.Start();
                ElementId new_type_id = floorType.Duplicate(origin_name + "_comments_" + comment).Id;
                FloorType newFloorType = floor.Document.GetElement(new_type_id) as FloorType;
                newFloorType.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS).Set(comment);
                floor.FloorType = floor.Document.GetElement(new_type_id) as FloorType;
                tr.Commit();
            }
        }

        // retrieval
        public static void Retrieval(Floor floor, string option)
        {
            // retrieve corresponding properties of the floor and display in the Revit UI
            switch (option)
            {
                case "Level":
                    TaskDialog.Show("Level", floor.get_Parameter(BuiltInParameter.LEVEL_PARAM).AsValueString());
                    break;
                case "Height Offset From Level":
                    TaskDialog.Show("HeightOffset", floor.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM).AsValueString());
                    break;
                case "Room Bounding":
                    TaskDialog.Show("RoomBounding", floor.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).AsValueString());
                    break;
                case "Structural":
                    TaskDialog.Show("Structural", floor.get_Parameter(BuiltInParameter.FLOOR_PARAM_IS_STRUCTURAL).AsValueString());
                    break;
                case "Slope":
                    TaskDialog.Show("Slope", floor.get_Parameter(BuiltInParameter.ROOF_SLOPE).AsValueString());
                    break;
                case "Comments":
                    TaskDialog.Show("Comments", floor.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsValueString());
                    break;
                case "Material":
                    TaskDialog.Show("Material", floor.Document.GetElement(floor.FloorType.GetCompoundStructure().GetLayers()[0].MaterialId).Name);
                    break;
                case "Color":
                    Color color = (floor.Document.GetElement(floor.FloorType.GetCompoundStructure().GetLayers()[0].MaterialId) as Material).Color;
                    TaskDialog.Show("Color (RGB)", color.Red.ToString() + " " + color.Green.ToString() + " " + color.Blue.ToString());
                    break;
                case "Thickness":
                    TaskDialog.Show("Thickness", floor.FloorType.GetCompoundStructure().GetLayers()[0].Width.ToString());
                    break;
                case "Function":
                    TaskDialog.Show("Function", floor.Document.GetElement(floor.FloorType.Id).get_Parameter(BuiltInParameter.FUNCTION_PARAM).AsValueString());
                    break;
                case "Type Comments":
                    TaskDialog.Show("Type Comments", floor.Document.GetElement(floor.FloorType.Id).get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS).AsValueString());
                    break;
                case "Perimeter":
                    TaskDialog.Show("Perimeter", floor.get_Parameter(BuiltInParameter.HOST_PERIMETER_COMPUTED).AsValueString());
                    break;
                case "Area":
                    TaskDialog.Show("Area", floor.get_Parameter(BuiltInParameter.HOST_AREA_COMPUTED).AsValueString());
                    break;
                case "Volumn":
                    TaskDialog.Show("Volumn", floor.get_Parameter(BuiltInParameter.HOST_VOLUME_COMPUTED).AsValueString());
                    break;
                case "Coordinate":
                    BoundingBoxXYZ bounding = floor.get_BoundingBox(floor.Document.ActiveView);
                    TaskDialog.Show("Coordinate", "Max: " + bounding.Max.ToString() + "\nMin: " + bounding.Min.ToString());
                    break;
            }
        }

        // deletion
        public static void Delete(Floor floor)
        {
            using (Transaction tr = new Transaction(floor.Document, "delete_floor"))
            {
                tr.Start();
                floor.Document.Delete(floor.Id);
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

            FloorType floorType = new FilteredElementCollector(doc)
                .OfClass(typeof(FloorType))
                .WhereElementIsElementType()
                .Cast<FloorType>()
                .FirstOrDefault(x => x.FamilyName == "Floor" && x.GetCompoundStructure().GetLayers().Count == 1);

            Floor floor = null;
            // create the floor
            using (Transaction tr = new Transaction(doc, "create_floor"))
            {
                tr.Start();
                floor = Floor.Create(doc, curveLoop_l, floorType.Id, level.Id);
                tr.Commit();
            }
            // modify based on the user arguments
            if (dic.ContainsKey("Height Offset From Level"))
            {
                ModifyHeightOffsetFromLevel(floor, double.Parse(dic["Height Offset From Level"]));
            }
            if (dic.ContainsKey("Room Bounding"))
            {
                ModifyRoomBounding(floor, int.Parse(dic["Room Bounding"]));
            }
            if (dic.ContainsKey("Structural"))
            {
                ModifyStructural(floor, int.Parse(dic["Structural"]));
            }
            if (dic.ContainsKey("Slope"))
            {
                ModifySlope(floor, double.Parse(dic["Slope"]));
            }
            if (dic.ContainsKey("Comments"))
            {
                ModifyComments(floor, dic["Comments"]);
            }
            if (dic.ContainsKey("Material"))
            {
                ModifyMaterial(floor, dic["Material"]);
            }
            if (dic.ContainsKey("Color"))
            {
                ModifyColor(floor, dic["Color"]);
            }
            if (dic.ContainsKey("Thickness"))
            {
                ModifyThickness(floor, double.Parse(dic["Thickness"]));
            }
            if (dic.ContainsKey("Function"))
            {
                ModifyFunction(floor, int.Parse(dic["Function"]));
            }
            if (dic.ContainsKey("Type Comments"))
            {
                ModifyTypeComments(floor, dic["Type Comments"]);
            }
        }
    }
}
