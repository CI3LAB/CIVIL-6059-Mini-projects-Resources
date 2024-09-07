using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using BIM;

namespace BIM
{
    internal class WallFunc
    {
        // modification
        public static void Modify(Wall wall, string property_name, string property_value)
        {
            switch (property_name)
            {
                case "Base Constraint":
                    ModifyBaseConstraint(wall, property_value);
                    break;
                case "Base Offset":
                    ModifyBaseOffset(wall, double.Parse(property_value));
                    break;
                case "Top Constraint":
                    ModifyTopConstraint(wall, property_value);
                    break;
                case "Top Offset":
                    ModifyTopOffset(wall, double.Parse(property_value));
                    break;
                case "Unconnected Height":
                    ModifyUnconnectedHeight(wall, double.Parse(property_value));
                    break;
                case "Room Bounding":
                    ModifyRoomBounding(wall, int.Parse(property_value));
                    break;
                case "Cross-Section":
                    ModifyCrossSection(wall, int.Parse(property_value));
                    break;
                case "Angle From Vertical":
                    ModifyAngelFromVertical(wall, double.Parse(property_value));
                    break;
                case "Structural":
                    ModifyStructural(wall, int.Parse(property_value));
                    break;
                case "Structural Usage":
                    ModifyStructuralUsage(wall, int.Parse(property_value));
                    break;
                case "Comments":
                    ModifyComments(wall, property_value);
                    break;
                case "Wrapping At Inserts":
                    ModifyWrappingAtInserts(wall, int.Parse(property_value));
                    break;
                case "Wrapping At Ends":
                    ModifyWrappingAtEnds(wall, int.Parse(property_value));
                    break;
                case "Width":
                    ModifyWidth(wall, double.Parse(property_value));
                    break;
                case "Function":
                    ModifyFunction(wall, int.Parse(property_value));
                    break;
                case "Material":
                    ModifyMaterial(wall, property_value);
                    break;
                case "Color":
                    ModifyColor(wall, property_value);
                    break;
                case "Type Comments":
                    ModifyTypeComments(wall, property_value);
                    break;
            }
        }

        // instance properties
        public static void ModifyBaseConstraint(Wall wall, string level_str)
        {
            // find the level id with the given name
            Level level = new FilteredElementCollector(wall.Document)
                .OfClass(typeof(Level))
                .Where(x => x.Name == level_str)
                .Cast<Level>()
                .FirstOrDefault();
            // modify the base constraint of the wall
            using (Transaction tr = new Transaction(wall.Document, "modify_wall_base_constraint"))
            {
                tr.Start();
                wall.get_Parameter(BuiltInParameter.WALL_BASE_CONSTRAINT).Set(level.Id);
                tr.Commit();
            }
        }

        public static void ModifyBaseOffset(Wall wall, double offset)
        {
            // modify the base offset of the wall
            // the metric of offest here is feet
            using (Transaction tr = new Transaction(wall.Document, "modify_wall_base_offset"))
            {
                tr.Start();
                wall.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET).Set(offset);
                tr.Commit();
            }
        }

        public static void ModifyTopConstraint(Wall wall, string level_str)
        {
            // find the level id with the given name
            Level level = new FilteredElementCollector(wall.Document)
                .OfClass(typeof(Level))
                .Where(x => x.Name == level_str)
                .Cast<Level>()
                .FirstOrDefault();
            // modify the top constraint of the wall
            using (Transaction tr = new Transaction(wall.Document, "modify_wall_top_constraint"))
            {
                tr.Start();
                if (level_str == "Unconnected")
                    wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(ElementId.InvalidElementId);
                else
                    wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(level.Id);
                tr.Commit();
            }
        }

        public static void ModifyTopOffset(Wall wall, double offset)
        {
            // modify the top offset of the wall
            // the metric of offest here is feet
            using (Transaction tr = new Transaction(wall.Document, "modify_wall_top_offset"))
            {
                tr.Start();
                wall.get_Parameter(BuiltInParameter.WALL_TOP_OFFSET).Set(offset);
                tr.Commit();
            }
        }

        public static void ModifyUnconnectedHeight(Wall wall, double height)
        {
            // modify the unconnectd height of the wall
            // the metric of height here is feet
            using (Transaction tr = new Transaction(wall.Document, "modify_wall_unconnected_height"))
            {
                tr.Start();
                wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).Set(height);
                tr.Commit();
            }
        }

        public static void ModifyRoomBounding(Wall wall, int rb)
        {
            // modify the value of room bounding property
            // 0->no, 1->yes
            using (Transaction tr = new Transaction(wall.Document, "modify_wall_room_bounding"))
            {
                tr.Start();
                wall.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(rb);
                tr.Commit();
            }
        }

        public static void ModifyCrossSection(Wall wall, int cs)
        {
            // modify the value of cross section property
            // 0->slanted, 1->vertical, 2->tapered
            using (Transaction tr = new Transaction(wall.Document, "modify_wall_cross_section"))
            {
                tr.Start();
                wall.get_Parameter(BuiltInParameter.WALL_CROSS_SECTION).Set(cs);
                tr.Commit();
            }
        }

        public static void ModifyAngelFromVertical(Wall wall, double angle)
        {
            // modify the angle from vertical of the wall
            // the metirc here is radian, 1 rad = 57.3 degree, degree is °
            using (Transaction tr = new Transaction(wall.Document, "modify_wall_angle_from_vertical"))
            {
                tr.Start();
                wall.get_Parameter(BuiltInParameter.WALL_SINGLE_SLANT_ANGLE_FROM_VERTICAL).Set(angle);
                tr.Commit();
            }
        }

        public static void ModifyStructural(Wall wall, int st)
        {
            // modify whether the wall is structural
            // 0->non-structural, 1->structural
            using (Transaction tr = new Transaction(wall.Document, "modify_wall_structural"))
            {
                tr.Start();
                wall.get_Parameter(BuiltInParameter.WALL_STRUCTURAL_SIGNIFICANT).Set(st);
                tr.Commit();
            }
        }

        public static void ModifyStructuralUsage(Wall wall, int su)
        {
            // modify the structural usage of the wall
            // 0->Non-structural, 1->Bearing, 2->Shear, 3->Structural combined
            using (Transaction tr = new Transaction(wall.Document, "modify_wall_structural_usage"))
            {
                tr.Start();
                wall.get_Parameter(BuiltInParameter.WALL_STRUCTURAL_USAGE_PARAM).Set(su);
                tr.Commit();
            }
        }

        public static void ModifyComments(Wall wall, string comment)
        {
            // modify the comments of the wall
            using (Transaction tr = new Transaction(wall.Document, "modify_wall_comments"))
            {
                tr.Start();
                wall.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set(comment);
                tr.Commit();
            }
        }

        // type properties
        public static void ModifyWrappingAtInserts(Wall wall, int option)
        {
            // Modify the wrapping at inserts property of the wall
            // 0->Do not wrap, 1->Exterior, 2->Interior, 3->Both
            // duplicate the wall type of the wall
            WallType wallType = wall.WallType;
            string origin_name = wallType.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(wall.Document, "modify_wall_wrapping_at_inserts"))
            {
                tr.Start();
                ElementId new_type_id = wallType.Duplicate(origin_name + "_wai_option_" + option.ToString()).Id;
                wall.Document.GetElement(new_type_id).get_Parameter(BuiltInParameter.WRAPPING_AT_INSERTS_PARAM).Set(option);
                wall.WallType = wall.Document.GetElement(new_type_id) as WallType;
                tr.Commit();
            }
        }

        public static void ModifyWrappingAtEnds(Wall wall, int option)
        {
            // Modify the wrapping at ends property of the wall
            // 0->Do not wrap, 1->Exterior, 2->Interior
            // duplicate the wall type of the wall
            WallType wallType = wall.WallType;
            string origin_name = wallType.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(wall.Document, "modify_wall_wrapping_at_ends"))
            {
                tr.Start();
                ElementId new_type_id = wallType.Duplicate(origin_name + "_wae_option_" + option.ToString()).Id;
                wall.Document.GetElement(new_type_id).get_Parameter(BuiltInParameter.WRAPPING_AT_ENDS_PARAM).Set(option);
                wall.WallType = wall.Document.GetElement(new_type_id) as WallType;
                tr.Commit();
            }
        }

        public static void ModifyWidth(Wall wall, double width)
        {
            // modify the width of the wall
            // the metric of width here is feet
            // duplicate the wall type of the wall
            WallType wallType = wall.WallType;
            string origin_name = wallType.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(wall.Document, "modify_wall_width"))
            {
                tr.Start();
                ElementId new_type_id = wallType.Duplicate(origin_name + "_width_" + width.ToString()).Id;
                WallType newWallType = wall.Document.GetElement(new_type_id) as WallType;
                CompoundStructure cs = wall.WallType.GetCompoundStructure();
                IList<CompoundStructureLayer> layers = cs.GetLayers();
                layers[0].Width = width;
                cs.SetLayers(layers);
                newWallType.SetCompoundStructure(cs);
                wall.WallType = wall.Document.GetElement(new_type_id) as WallType;
                tr.Commit();
            }
        }

        public static void ModifyFunction(Wall wall, int fun)
        {
            // modify the function of the wall type
            // 0->Interior, 1->Exterior, 2->Foundation, 3->Retaining, 4->Soffit, 5->Core-shaft
            // duplicate the wall type of the wall
            WallType wallType = wall.WallType;
            string origin_name = wallType.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(wall.Document, "modify_wall_function"))
            {
                tr.Start();
                ElementId new_type_id = wallType.Duplicate(origin_name + "_function_" + fun.ToString()).Id;
                wall.Document.GetElement(new_type_id).get_Parameter(BuiltInParameter.FUNCTION_PARAM).Set(fun);
                wall.WallType = wall.Document.GetElement(new_type_id) as WallType;
                tr.Commit();
            }
        }

        public static void ModifyMaterial(Wall wall, string material_name)
        {
            // modify the material of the wall type
            // duplicate the wall type of the wall
            WallType wallType = wall.WallType;
            string origin_name = wallType.Name;
            // get all the mateirals
            FilteredElementCollector collector = new FilteredElementCollector(wall.Document);
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
            using (Transaction tr = new Transaction(wall.Document, "modify_wall_material"))
            {
                tr.Start();
                ElementId new_type_id = wallType.Duplicate(origin_name + "_material_" + material_name).Id;
                WallType newWallType = wall.Document.GetElement(new_type_id) as WallType;
                CompoundStructure cs = wall.WallType.GetCompoundStructure();
                IList<CompoundStructureLayer> layers = cs.GetLayers();
                layers[0].MaterialId = materialIds[min_index];
                cs.SetLayers(layers);
                newWallType.SetCompoundStructure(cs);
                wall.WallType = wall.Document.GetElement(new_type_id) as WallType;
                tr.Commit();
            }
        }

        public static void ModifyColor(Wall wall, string color)
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
            // duplicate the wall type of the wall
            WallType wallType = wall.WallType;
            string origin_name = wallType.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(wall.Document, "modify_wall_color"))
            {
                tr.Start();
                ElementId new_type_id = wallType.Duplicate(origin_name + "_color_" + color).Id;
                WallType newWallType = wall.Document.GetElement(new_type_id) as WallType;
                CompoundStructure cs = wall.WallType.GetCompoundStructure();
                IList<CompoundStructureLayer> layers = cs.GetLayers();
                Material material = wall.Document.GetElement(layers[0].MaterialId) as Material;
                material = material.Duplicate(material.Name + "_color_" + color);
                material.Color = color_map[color];
                layers[0].MaterialId = material.Id;
                cs.SetLayers(layers);
                newWallType.SetCompoundStructure(cs);
                wall.WallType = wall.Document.GetElement(new_type_id) as WallType;
                tr.Commit();
            }
        }

        public static void ModifyTypeComments(Wall wall, string comment)
        {
            // modify the comments of the wall type
            // duplicate the wall type of the wall
            WallType wallType = wall.WallType;
            string origin_name = wallType.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(wall.Document, "modify_wall_type_comments"))
            {
                tr.Start();
                ElementId new_type_id = wallType.Duplicate(origin_name + "_type_comments_" + comment).Id;
                wall.Document.GetElement(new_type_id).get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS).Set(comment);
                wall.WallType = wall.Document.GetElement(new_type_id) as WallType;
                tr.Commit();
            }
        }

        // retrieval
        public static void Retrieval(Wall wall, string option)
        {
            // retrieve corresponding properties of the wall and display in the Revit UI
            switch (option)
            {
                case "Base Constraint":
                    TaskDialog.Show("Base Constraint", wall.get_Parameter(BuiltInParameter.WALL_BASE_CONSTRAINT).AsValueString());
                    break;
                case "Base Offset":
                    TaskDialog.Show("Base Offset", wall.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET).AsValueString());
                    break;
                case "Top Constraint":
                    TaskDialog.Show("Top Constraint", wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).AsValueString());
                    break;
                case "Top Offset":
                    TaskDialog.Show("Top Offset", wall.get_Parameter(BuiltInParameter.WALL_TOP_OFFSET).AsValueString());
                    break;
                case "Unconnected Height":
                    TaskDialog.Show("Unconnected Height", wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).AsValueString());
                    break;
                case "Room Bounding":
                    TaskDialog.Show("Room Bounding", wall.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).AsValueString());
                    break;
                case "Cross Section":
                    TaskDialog.Show("Cross Section", wall.get_Parameter(BuiltInParameter.WALL_CROSS_SECTION).AsValueString());
                    break;
                case "Angle From Vertical":
                    TaskDialog.Show("Angle From Vertical", wall.get_Parameter(BuiltInParameter.WALL_SINGLE_SLANT_ANGLE_FROM_VERTICAL).AsValueString());
                    break;
                case "Structural":
                    TaskDialog.Show("Structural", wall.get_Parameter(BuiltInParameter.WALL_STRUCTURAL_SIGNIFICANT).AsValueString());
                    break;
                case "Structural Usage":
                    TaskDialog.Show("Structural Usage", wall.get_Parameter(BuiltInParameter.WALL_STRUCTURAL_USAGE_PARAM).AsValueString());
                    break;
                case "Comments":
                    TaskDialog.Show("Comments", wall.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString());
                    break;
                case "Wrapping At Inserts":
                    TaskDialog.Show("Wrapping At Inserts", wall.WallType.get_Parameter(BuiltInParameter.WRAPPING_AT_INSERTS_PARAM).AsValueString());
                    break;
                case "Wrapping At Ends":
                    TaskDialog.Show("Wrapping At Ends", wall.WallType.get_Parameter(BuiltInParameter.WRAPPING_AT_ENDS_PARAM).AsValueString());
                    break;
                case "Width":
                    TaskDialog.Show("Width", wall.WallType.GetCompoundStructure().GetLayers()[0].Width.ToString());
                    break;
                case "Function":
                    TaskDialog.Show("Function", wall.WallType.get_Parameter(BuiltInParameter.FUNCTION_PARAM).AsValueString());
                    break;
                case "Material":
                    TaskDialog.Show("Material", wall.Document.GetElement(wall.WallType.GetCompoundStructure().GetLayers()[0].MaterialId).Name);
                    break;
                case "Color":
                    Color color = (wall.Document.GetElement(wall.WallType.GetCompoundStructure().GetLayers()[0].MaterialId) as Material).Color;
                    TaskDialog.Show("Color (RGB)", color.Red.ToString() + " " + color.Green.ToString() + " " + color.Blue.ToString());
                    break;
                case "Type Comments":
                    TaskDialog.Show("Type Comments", wall.WallType.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS).AsString());
                    break;
                case "Length":
                    TaskDialog.Show("Length", wall.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsValueString());
                    break;
                case "Area":
                    TaskDialog.Show("Area", wall.get_Parameter(BuiltInParameter.HOST_AREA_COMPUTED).AsValueString());
                    break;
                case "Volumn":
                    TaskDialog.Show("Volumn", wall.get_Parameter(BuiltInParameter.HOST_VOLUME_COMPUTED).AsValueString());
                    break;
                case "Coordinate":
                    BoundingBoxXYZ bounding = wall.get_BoundingBox(wall.Document.ActiveView);
                    TaskDialog.Show("Coordinate", "Max: " + bounding.Max.ToString() + "\nMin: " + bounding.Min.ToString());
                    break;
            }
        }

        // deletion
        public static void Delete(Wall wall)
        {
            using (Transaction tr = new Transaction(wall.Document, "delete_wall"))
            {
                tr.Start();
                wall.Document.Delete(wall.Id);
                tr.Commit();
            }
        }

        // creation
        public static void Create(Document doc, XYZ p1, XYZ p2, Dictionary<string, string> dic)
        {
            Level level = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .First() as Level;
            int structural = 0;
            if (dic.ContainsKey("Base Constraint"))
            {
                string base_constraint = dic["Base Constraint"];
                level = new FilteredElementCollector(doc)
                    .OfClass(typeof(Level))
                    .Where(x => x.Name == base_constraint)
                    .Cast<Level>()
                    .FirstOrDefault();
            }
            if (dic.ContainsKey("Structural"))
            {
                structural = int.Parse(dic["Structural"]);
            }

            WallType wallType = new FilteredElementCollector(doc)
                .OfClass(typeof(WallType))
                .WhereElementIsElementType()
                .Cast<WallType>()
                .FirstOrDefault(x => x.FamilyName == "Basic Wall" && x.GetCompoundStructure().GetLayers().Count == 1);

            Wall wall = null;
            // create the wall
            using (Transaction tr = new Transaction(doc, "create_wall"))
            {
                tr.Start();
                wall = Wall.Create(doc, Line.CreateBound(p1, p2), level.Id, false);
                wall.WallType = wallType;
                tr.Commit();
            }
            // modify based on the user arguments
            if (dic.ContainsKey("Base Offset"))
            {
                ModifyBaseOffset(wall, double.Parse(dic["Base Offset"]));
            }
            if (dic.ContainsKey("Top Constraint"))
            {
                ModifyTopConstraint(wall, dic["Top Constraint"]);
            }
            if (dic.ContainsKey("Top Offset"))
            {
                ModifyTopOffset(wall, double.Parse(dic["Top Offset"]));
            }
            if (dic.ContainsKey("Unconnected Height"))
            {
                ModifyUnconnectedHeight(wall, double.Parse(dic["Unconnected Height"]));
            }
            if (dic.ContainsKey("Room Bounding"))
            {
                ModifyRoomBounding(wall, int.Parse(dic["Room Bounding"]));
            }
            if (dic.ContainsKey("Cross Section"))
            {
                ModifyCrossSection(wall, int.Parse(dic["Cross Section"]));
            }
            if (dic.ContainsKey("Angle From Vertical"))
            {
                ModifyAngelFromVertical(wall, double.Parse(dic["Angle From Vertical"]));
            }
            if (dic.ContainsKey("Structural Usage"))
            {
                ModifyStructuralUsage(wall, int.Parse(dic["Structural Usage"]));
            }
            if (dic.ContainsKey("Comments"))
            {
                ModifyComments(wall, dic["Comments"]);
            }
            if (dic.ContainsKey("Wrapping At Inserts"))
            {
                ModifyWrappingAtInserts(wall, int.Parse(dic["Wrapping At Inserts"]));
            }
            if (dic.ContainsKey("Wrapping At Ends"))
            {
                ModifyWrappingAtEnds(wall, int.Parse(dic["Wrapping At Ends"]));
            }
            if (dic.ContainsKey("Width"))
            {
                ModifyWidth(wall, double.Parse(dic["Width"]));
            }
            if (dic.ContainsKey("Function"))
            {
                ModifyFunction(wall, int.Parse(dic["Function"]));
            }
            if (dic.ContainsKey("Material"))
            {
                ModifyMaterial(wall, dic["Material"]);
            }
            if (dic.ContainsKey("Color"))
            {
                ModifyColor(wall, dic["Color"]);
            }
            if (dic.ContainsKey("Type Comments"))
            {
                ModifyTypeComments(wall, dic["Type Comments"]);
            }
        }
    }
}
