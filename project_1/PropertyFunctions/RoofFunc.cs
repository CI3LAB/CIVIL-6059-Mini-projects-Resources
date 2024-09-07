using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;

namespace BIM
{
    internal class RoofFunc
    {
        // modification
        public static void Modify(FootPrintRoof roof, string property_name, string property_value)
        {
            switch (property_name)
            {
                case "Base Level":
                    ModifyBaseLevel(roof, property_value);
                    break;
                case "Room Bounding":
                    ModifyRoomBounding(roof, int.Parse(property_value));
                    break;
                case "Base Offset From Level":
                    ModifyBaseOffsetFromLevel(roof, double.Parse(property_value));
                    break;
                case "Rafter Cut":
                    ModifyRafterCut(roof, int.Parse(property_value));
                    break;
                case "Fascia Depth":
                    ModifyFasciaDepth(roof, double.Parse(property_value));
                    break;
                case "Rafter Or Truss":
                    ModifyRafterOrTruss(roof, int.Parse(property_value));
                    break;
                case "Slope":
                    ModifySlope(roof, double.Parse(property_value));
                    break;
                case "Comments":
                    ModifyComments(roof, property_value);
                    break;
                case "Material":
                    ModifyMaterial(roof, property_value);
                    break;
                case "Color":
                    ModifyColor(roof, property_value);
                    break;
                case "Thickness":
                    ModifyThickness(roof, double.Parse(property_value));
                    break;
                case "Type Comments":
                    ModifyTypeComments(roof, property_value);
                    break;
            }
        }

        // instance properties
        public static void ModifyBaseLevel(FootPrintRoof roof, string level_str)
        {
            // find the level id with the given name
            Level level = new FilteredElementCollector(roof.Document)
                .OfClass(typeof(Level))
                .Where(x => x.Name == level_str)
                .Cast<Level>()
                .FirstOrDefault();
            // modify the base level of the roof
            using (Transaction tr = new Transaction(roof.Document, "modify_roof_base_level"))
            {
                tr.Start();
                roof.get_Parameter(BuiltInParameter.ROOF_BASE_LEVEL_PARAM).Set(level.Id);
                tr.Commit();
            }
        }

        public static void ModifyRoomBounding(FootPrintRoof roof, int option)
        {
            // modify the value of room bounding property
            // 0->no, 1->yes
            using (Transaction tr = new Transaction(roof.Document, "modify_roof_room_bounding"))
            {
                tr.Start();
                roof.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(option);
                tr.Commit();
            }
        }

        public static void ModifyBaseOffsetFromLevel(FootPrintRoof roof, double offest)
        {
            // modify the value of base offset from level
            using (Transaction tr = new Transaction(roof.Document, "modify_roof_base_offset_from_level"))
            {
                tr.Start();
                roof.get_Parameter(BuiltInParameter.ROOF_LEVEL_OFFSET_PARAM).Set(offest);
                tr.Commit();
            }
        }

        public static void ModifyRafterCut(FootPrintRoof roof, int option)
        {
            // modify the value of rafter cut property
            // 33615->Plumb Cut, 33619->Two Cut - Plumb, 33618->Two Cut - Square
            using (Transaction tr = new Transaction(roof.Document, "modify_roof_rafter_cut"))
            {
                tr.Start();
                roof.get_Parameter(BuiltInParameter.ROOF_EAVE_CUT_PARAM).Set(option);
                tr.Commit();
            }
        }

        public static void ModifyFasciaDepth(FootPrintRoof roof, double depth)
        {
            // modify the value of fascia depth
            using (Transaction tr = new Transaction(roof.Document, "modify_roof_fascia_depth"))
            {
                tr.Start();
                roof.get_Parameter(BuiltInParameter.FASCIA_DEPTH_PARAM).Set(depth);
                tr.Commit();
            }
        }

        public static void ModifyRafterOrTruss(FootPrintRoof roof, int option)
        {
            // modify the value of rafter or truss property
            // 0->Truss, 1->Rafter
            using (Transaction tr = new Transaction(roof.Document, "modify_roof_rafter_or_truss"))
            {
                tr.Start();
                roof.get_Parameter(BuiltInParameter.ROOF_RAFTER_OR_TRUSS_PARAM).Set(option);
                tr.Commit();
            }
        }

        public static void ModifySlope(FootPrintRoof roof, double slope)
        {
            // modify the value of slope
            // the metirc here is the value of tan(degree), where degree is shown in the Revit UI
            using (Transaction tr = new Transaction(roof.Document, "modify_roof_slope"))
            {
                tr.Start();
                roof.get_Parameter(BuiltInParameter.ROOF_SLOPE).Set(slope);
                tr.Commit();
            }
        }

        public static void ModifyComments(FootPrintRoof roof, string comment)
        {
            // modify the comments of the roof
            using (Transaction tr = new Transaction(roof.Document, "modify_roof_comments"))
            {
                tr.Start();
                roof.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set(comment);
                tr.Commit();
            }
        }

        // type properties
        public static void ModifyMaterial(FootPrintRoof roof, string material_name)
        {
            // modify the material of the roof type
            // duplicate the roof type of the roof
            RoofType roofType = roof.RoofType;
            string origin_name = roofType.Name;
            // get all the mateirals
            FilteredElementCollector collector = new FilteredElementCollector(roof.Document);
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
            using (Transaction tr = new Transaction(roof.Document, "modify_roof_material"))
            {
                tr.Start();
                ElementId new_type_id = roofType.Duplicate(origin_name + "_material_" + material_name).Id;
                RoofType newRoofType = roof.Document.GetElement(new_type_id) as RoofType;
                CompoundStructure cs = roof.RoofType.GetCompoundStructure();
                IList<CompoundStructureLayer> layers = cs.GetLayers();
                layers[0].MaterialId = materialIds[min_index];
                cs.SetLayers(layers);
                newRoofType.SetCompoundStructure(cs);
                roof.RoofType = roof.Document.GetElement(new_type_id) as RoofType;
                tr.Commit();
            }
        }

        public static void ModifyColor(FootPrintRoof roof, string color)
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
            // duplicate the roof type of the roof
            RoofType roofType = roof.RoofType;
            string origin_name = roofType.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(roof.Document, "modify_roof_color"))
            {
                tr.Start();
                ElementId new_type_id = roofType.Duplicate(origin_name + "_color_" + color).Id;
                RoofType newRoofType = roof.Document.GetElement(new_type_id) as RoofType;
                CompoundStructure cs = roof.RoofType.GetCompoundStructure();
                IList<CompoundStructureLayer> layers = cs.GetLayers();
                Material material = roof.Document.GetElement(layers[0].MaterialId) as Material;
                material = material.Duplicate(material.Name + "_color_" + color);
                material.Color = color_map[color];
                layers[0].MaterialId = material.Id;
                cs.SetLayers(layers);
                newRoofType.SetCompoundStructure(cs);
                roof.RoofType = roof.Document.GetElement(new_type_id) as RoofType;
                tr.Commit();
            }
        }

        public static void ModifyThickness(FootPrintRoof roof, double thickness)
        {
            // modify the thickness of the roof
            // the metric of thickness here is feet
            // duplicate the roof type of the roof
            RoofType roofType = roof.RoofType;
            string origin_name = roofType.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(roof.Document, "modify_roof_thickness"))
            {
                tr.Start();
                ElementId new_type_id = roofType.Duplicate(origin_name + "_thickness_" + thickness.ToString()).Id;
                RoofType newRoofType = roof.Document.GetElement(new_type_id) as RoofType;
                CompoundStructure cs = roof.RoofType.GetCompoundStructure();
                IList<CompoundStructureLayer> layers = cs.GetLayers();
                layers[0].Width = thickness;
                cs.SetLayers(layers);
                newRoofType.SetCompoundStructure(cs);
                roof.RoofType = roof.Document.GetElement(new_type_id) as RoofType;
                tr.Commit();
            }
        }

        public static void ModifyTypeComments(FootPrintRoof roof, string comment)
        {
            // modify the comments of the roof type
            // duplicate the roof type of the roof
            RoofType roofType = roof.RoofType;
            string origin_name = roofType.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(roof.Document, "modify_roof_type_comments"))
            {
                tr.Start();
                ElementId new_type_id = roofType.Duplicate(origin_name + "_type_comments_" + comment).Id;
                roof.Document.GetElement(new_type_id).get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS).Set(comment);
                roof.RoofType = roof.Document.GetElement(new_type_id) as RoofType;
                tr.Commit();
            }
        }

        // retrieval
        public static void Retrieval(FootPrintRoof roof, string option)
        {
            // retrieve corresponding properties of the roof and display in the Revit UI
            switch (option)
            {
                case "Base Level":
                    TaskDialog.Show("Base Constraint", roof.get_Parameter(BuiltInParameter.ROOF_BASE_LEVEL_PARAM).AsValueString());
                    break;
                case "Room Bounding":
                    TaskDialog.Show("Room Bounding", roof.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).AsValueString());
                    break;
                case "Base Offset From Level":
                    TaskDialog.Show("Base Offset From Level", roof.get_Parameter(BuiltInParameter.ROOF_LEVEL_OFFSET_PARAM).AsValueString());
                    break;
                case "Rafter Cut":
                    TaskDialog.Show("Rafter Cut", roof.get_Parameter(BuiltInParameter.ROOF_EAVE_CUT_PARAM).AsValueString());
                    break;
                case "Fascia Depth":
                    TaskDialog.Show("Fascia Depth", roof.get_Parameter(BuiltInParameter.FASCIA_DEPTH_PARAM).AsValueString());
                    break;
                case "Rafter Or Truss":
                    TaskDialog.Show("Rafter Or Truss", roof.get_Parameter(BuiltInParameter.ROOF_RAFTER_OR_TRUSS_PARAM).AsValueString());
                    break;
                case "Slope":
                    TaskDialog.Show("Slope", roof.get_Parameter(BuiltInParameter.ROOF_SLOPE).AsValueString());
                    break;
                case "Comments":
                    TaskDialog.Show("Comments", roof.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsValueString());
                    break;
                case "Material":
                    CompoundStructure cs = roof.RoofType.GetCompoundStructure();
                    IList<CompoundStructureLayer> layers = cs.GetLayers();
                    Material material = roof.Document.GetElement(layers[0].MaterialId) as Material;
                    TaskDialog.Show("Material", material.Name);
                    break;
                case "Color":
                    CompoundStructure cs1 = roof.RoofType.GetCompoundStructure();
                    IList<CompoundStructureLayer> layers1 = cs1.GetLayers();
                    Material material1 = roof.Document.GetElement(layers1[0].MaterialId) as Material;
                    TaskDialog.Show("Color (RGB)", material1.Color.Red.ToString() + " " + material1.Color.Green.ToString() + " " + material1.Color.Blue.ToString());
                    break;
                case "Thickness":
                    CompoundStructure cs2 = roof.RoofType.GetCompoundStructure();
                    IList<CompoundStructureLayer> layers2 = cs2.GetLayers();
                    TaskDialog.Show("Thickness", layers2[0].Width.ToString());
                    break;
                case "Type Comments":
                    TaskDialog.Show("Type Comments", roof.RoofType.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS).AsValueString());
                    break;
                case "Maximum Ridge Height":
                    TaskDialog.Show("Maximum Ridge Height", roof.get_Parameter(BuiltInParameter.ACTUAL_MAX_RIDGE_HEIGHT_PARAM).AsValueString());
                    break;
                case "Volumn":
                    TaskDialog.Show("Volumn", roof.get_Parameter(BuiltInParameter.HOST_VOLUME_COMPUTED).AsValueString());
                    break;
                case "Area":
                    TaskDialog.Show("Area", roof.get_Parameter(BuiltInParameter.HOST_AREA_COMPUTED).AsValueString());
                    break;
                case "Coordinate":
                    BoundingBoxXYZ bounding = roof.get_BoundingBox(roof.Document.ActiveView);
                    TaskDialog.Show("Coordinate", "Max: " + bounding.Max.ToString() + "\nMin: " + bounding.Min.ToString());
                    break;
            }
        }

        // deletion
        public static void Delete(FootPrintRoof roof)
        {
            using (Transaction tr = new Transaction(roof.Document, "delete_roof"))
            {
                tr.Start();
                roof.Document.Delete(roof.Id);
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

            CurveArray curveArray = new CurveArray();
            curveArray.Append(line_1);
            curveArray.Append(line_2);
            curveArray.Append(line_3);
            curveArray.Append(line_4);

            Level level = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .First() as Level;
            if (dic.ContainsKey("Base Level"))
            {
                string level_name = dic["Base Level"];
                level = new FilteredElementCollector(doc)
                    .OfClass(typeof(Level))
                    .Where(x => x.Name == level_name)
                    .Cast<Level>()
                    .FirstOrDefault();
            }

            RoofType roofType = new FilteredElementCollector(doc)
                .OfClass(typeof(RoofType))
                .WhereElementIsElementType()
                .Cast<RoofType>()
                .FirstOrDefault(x => x.FamilyName == "Basic Roof" && x.GetCompoundStructure().GetLayers().Count == 1);
            FootPrintRoof roof = null;
            // create the floor
            using (Transaction tr = new Transaction(doc, "create_roof"))
            {
                tr.Start();
                ModelCurveArray footPrintToModelCurveMapping = new ModelCurveArray();
                roof = doc.Create.NewFootPrintRoof(curveArray, level, roofType, out footPrintToModelCurveMapping);
                tr.Commit();
            }
            // modify based on the user arguments
            if (dic.ContainsKey("Room Bounding"))
            {
                ModifyRoomBounding(roof, int.Parse(dic["Room Bounding"]));
            }
            if (dic.ContainsKey("Base Offset From Level"))
            {
                ModifyBaseOffsetFromLevel(roof, double.Parse(dic["Base Offset From Level"]));
            }
            if (dic.ContainsKey("Rafter Cut"))
            {
                ModifyRafterCut(roof, int.Parse(dic["Rafter Cut"]));
            }
            if (dic.ContainsKey("Fascia Depth"))
            {
                ModifyFasciaDepth(roof, double.Parse(dic["Fascia Depth"]));
            }
            if (dic.ContainsKey("Rafter Or Truss"))
            {
                ModifyRafterOrTruss(roof, int.Parse(dic["Rafter Or Truss"]));
            }
            if (dic.ContainsKey("Slope"))
            {
                ModifySlope(roof, double.Parse(dic["Slope"]));
            }
            if (dic.ContainsKey("Comments"))
            {
                ModifyComments(roof, dic["Comments"]);
            }
            if (dic.ContainsKey("Material"))
            {
                ModifyMaterial(roof, dic["Material"]);
            }
            if (dic.ContainsKey("Color"))
            {
                ModifyColor(roof, dic["Color"]);
            }
            if (dic.ContainsKey("Thickness"))
            {
                ModifyThickness(roof, double.Parse(dic["Thickness"]));
            }
            if (dic.ContainsKey("Type Comments"))
            {
                ModifyTypeComments(roof, dic["Type Comments"]);
            }
        }
    }
}
