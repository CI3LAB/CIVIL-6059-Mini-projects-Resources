using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIM
{
    internal class ColumnFunc
    {
        // modification
        public static void Modify(FamilyInstance column, string property_name, string property_value)
        {
            switch (property_name)
            {
                case "Base Level":
                    ModifyBaseLevel(column, property_value);
                    break;
                case "Base Offset":
                    ModifyBaseOffset(column, double.Parse(property_value));
                    break;
                case "Top Level":
                    ModifyTopLevel(column, property_value);
                    break;
                case "Top Offset":
                    ModifyTopOffset(column, double.Parse(property_value));
                    break;
                case "Moves With Grids":
                    ModifyMovesWithGrid(column, int.Parse(property_value));
                    break;
                case "Room Bounding":
                    ModifyRoomBounding(column, int.Parse(property_value));
                    break;
                case "Comments":
                    ModifyComments(column, property_value);
                    break;
                case "Material":
                    ModifyMaterial(column, property_value);
                    break;
                case "Color":
                    ModifyColor(column, property_value);
                    break;
                case "Depth":
                    ModifyDepth(column, double.Parse(property_value));
                    break;
                case "Offset Base":
                    ModifyOffsetBase(column, double.Parse(property_value));
                    break;
                case "Offset Top":
                    ModifyOffsetTop(column, double.Parse(property_value));
                    break;
                case "Width":
                    ModifyWidth(column, double.Parse(property_value));
                    break;
                case "Type Comments":
                    ModifyTypeComments(column, property_value);
                    break;
            }
        }

        // instance properties
        public static void ModifyBaseLevel(FamilyInstance column, string level_str)
        {
            // find the level id with the given name
            Level level = new FilteredElementCollector(column.Document)
                .OfClass(typeof(Level))
                .Where(x => x.Name == level_str)
                .Cast<Level>()
                .FirstOrDefault();
            // modify the level of the door
            using (Transaction tr = new Transaction(column.Document, "modify_column_base_level"))
            {
                tr.Start();
                column.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_PARAM).Set(level.Id);
                tr.Commit();
            }
        }

        public static void ModifyBaseOffset(FamilyInstance column, double offset)
        {
            // modify the base offset
            // the metric of offest here is feet
            using (Transaction tr = new Transaction(column.Document, "modify_column_base_offset"))
            {
                tr.Start();
                column.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM).Set(offset);
                tr.Commit();
            }
        }

        public static void ModifyTopLevel(FamilyInstance column, string level_str)
        {
            // find the level id with the given name
            Level level = new FilteredElementCollector(column.Document)
                .OfClass(typeof(Level))
                .Where(x => x.Name == level_str)
                .Cast<Level>()
                .FirstOrDefault();
            // modify the level of the door
            using (Transaction tr = new Transaction(column.Document, "modify_column_top_level"))
            {
                tr.Start();
                column.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(level.Id);
                tr.Commit();
            }
        }

        public static void ModifyTopOffset(FamilyInstance column, double offset)
        {
            // modify the base offset
            // the metric of offest here is feet
            using (Transaction tr = new Transaction(column.Document, "modify_column_top_offset"))
            {
                tr.Start();
                column.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).Set(offset);
                tr.Commit();
            }
        }

        public static void ModifyMovesWithGrid(FamilyInstance column, int mwg)
        {
            // modify the value of room bounding property
            // 0->no, 1->yes
            using (Transaction tr = new Transaction(column.Document, "modify_column_moves_with_grid"))
            {
                tr.Start();
                column.get_Parameter(BuiltInParameter.INSTANCE_MOVES_WITH_GRID_PARAM).Set(mwg);
                tr.Commit();
            }
        }

        public static void ModifyRoomBounding(FamilyInstance column, int rb)
        {
            // modify the value of room bounding property
            // 0->no, 1->yes
            using (Transaction tr = new Transaction(column.Document, "modify_column_room_bounding"))
            {
                tr.Start();
                column.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(rb);
                tr.Commit();
            }
        }

        public static void ModifyComments(FamilyInstance column, string comment)
        {
            // modify the comments of the column
            using (Transaction tr = new Transaction(column.Document, "modify_column_comments"))
            {
                tr.Start();
                column.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set(comment);
                tr.Commit();
            }
        }

        // type properties
        public static void ModifyMaterial(FamilyInstance column, string material_name)
        {
            // modify the material of the column
            // duplicate the symbol of the column
            FamilySymbol familySymbol = column.Symbol;
            string origin_name = familySymbol.Name;
            // get all the mateirals
            FilteredElementCollector collector = new FilteredElementCollector(column.Document);
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
            using (Transaction tr = new Transaction(column.Document, "modify_column_material"))
            {
                tr.Start();
                ElementId new_type_id = familySymbol.Duplicate(origin_name + "_column_material_" + material_name).Id;
                FamilySymbol newSymbol = column.Document.GetElement(new_type_id) as FamilySymbol;

                foreach (Parameter parameter in newSymbol.Parameters)
                {
                    if (parameter.Definition.Name == "Material") // 材质
                    {
                        Material columnMaterial = column.Document.GetElement(materialIds[min_index]) as Material;
                        parameter.Set(columnMaterial.Id);
                        column.Symbol = newSymbol;
                    }
                }

                tr.Commit();
            }
        }

        public static void ModifyColor(FamilyInstance column, string color)
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
            // duplicate the symbol of the column
            FamilySymbol familySymbol = column.Symbol;
            string origin_name = familySymbol.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(column.Document, "modify_column_color"))
            {
                tr.Start();
                ElementId new_type_id = familySymbol.Duplicate(origin_name + "_column_color_" + color).Id;
                FamilySymbol newSymbol = column.Document.GetElement(new_type_id) as FamilySymbol;

                foreach (Parameter parameter in newSymbol.Parameters)
                {
                    if (parameter.Definition.Name == "Material") // 材质
                    {
                        Material columnMaterial = column.Document.GetElement(parameter.AsElementId()) as Material;
                        columnMaterial = columnMaterial.Duplicate(columnMaterial.Name + "_column_color_" + color);
                        columnMaterial.Color = color_map[color];
                        parameter.Set(columnMaterial.Id);
                        column.Symbol = newSymbol;
                    }
                }

                tr.Commit();
            }
        }

        public static void ModifyDepth(FamilyInstance column, double depth)
        {
            // modify the depth of the column
            // duplicate the symbol of the column
            FamilySymbol familySymbol = column.Symbol;
            string origin_name = familySymbol.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(column.Document, "modify_column_depth"))
            {
                tr.Start();
                ElementId new_type_id = familySymbol.Duplicate(origin_name + "_column_depth_" + depth.ToString()).Id;
                FamilySymbol newSymbol = column.Document.GetElement(new_type_id) as FamilySymbol;

                foreach (Parameter parameter in newSymbol.Parameters)
                {
                    if (parameter.Definition.Name == "Depth") // 深度
                    {
                        parameter.Set(depth);
                        column.Symbol = newSymbol;
                    }
                }

                tr.Commit();
            }
        }

        public static void ModifyOffsetBase(FamilyInstance column, double offset)
        {
            // modify the offset base of the column
            // duplicate the symbol of the column
            FamilySymbol familySymbol = column.Symbol;
            string origin_name = familySymbol.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(column.Document, "modify_column_offset_base"))
            {
                tr.Start();
                ElementId new_type_id = familySymbol.Duplicate(origin_name + "_column_offset_base_" + offset.ToString()).Id;
                FamilySymbol newSymbol = column.Document.GetElement(new_type_id) as FamilySymbol;

                foreach (Parameter parameter in newSymbol.Parameters)
                {
                    if (parameter.Definition.Name == "Offset Base") // 偏移基准
                    {
                        parameter.Set(offset);
                        column.Symbol = newSymbol;
                    }
                }

                tr.Commit();
            }
        }

        public static void ModifyOffsetTop(FamilyInstance column, double offset)
        {
            // modify the offset top of the column
            // duplicate the symbol of the column
            FamilySymbol familySymbol = column.Symbol;
            string origin_name = familySymbol.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(column.Document, "modify_column_offset_top"))
            {
                tr.Start();
                ElementId new_type_id = familySymbol.Duplicate(origin_name + "_column_offset_top_" + offset.ToString()).Id;
                FamilySymbol newSymbol = column.Document.GetElement(new_type_id) as FamilySymbol;

                foreach (Parameter parameter in newSymbol.Parameters)
                {
                    if (parameter.Definition.Name == "Offset Top") // 偏移顶部
                    {
                        parameter.Set(offset);
                        column.Symbol = newSymbol;
                    }
                }

                tr.Commit();
            }
        }

        public static void ModifyWidth(FamilyInstance column, double width)
        {
            // modify the width of the column
            // duplicate the symbol of the column
            FamilySymbol familySymbol = column.Symbol;
            string origin_name = familySymbol.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(column.Document, "modify_column_width"))
            {
                tr.Start();
                ElementId new_type_id = familySymbol.Duplicate(origin_name + "_column_width_" + width.ToString()).Id;
                FamilySymbol newSymbol = column.Document.GetElement(new_type_id) as FamilySymbol;

                foreach (Parameter parameter in newSymbol.Parameters)
                {
                    if (parameter.Definition.Name == "Width") // 宽度
                    {
                        parameter.Set(width);
                        column.Symbol = newSymbol;
                    }
                }

                tr.Commit();
            }
        }

        public static void ModifyTypeComments(FamilyInstance column, string comment)
        {
            // modify the comments of the column type
            // duplicate the type
            FamilySymbol familySymbol = column.Symbol;
            string origin_name = familySymbol.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(column.Document, "modify_column_type_comments"))
            {
                tr.Start();
                ElementId new_type_id = familySymbol.Duplicate(origin_name + "_type_comments_" + comment).Id;
                column.Document.GetElement(new_type_id).get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS).Set(comment);
                column.Symbol = column.Document.GetElement(new_type_id) as FamilySymbol;
                tr.Commit();
            }
        }

        // retrieval
        public static void Retrieval(FamilyInstance column, string option)
        {
            // retrieve corresponding properties of the column and display in the Revit UI
            switch (option)
            {
                case "Base Level":
                    TaskDialog.Show("Base Level", column.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_PARAM).AsValueString());
                    break;
                case "Base Offset":
                    TaskDialog.Show("Base Offset", column.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM).AsValueString());
                    break;
                case "Top Level":
                    TaskDialog.Show("Top Level", column.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).AsValueString());
                    break;
                case "Top Offset":
                    TaskDialog.Show("Top Offset", column.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).AsValueString());
                    break;
                case "Moves With Grid":
                    TaskDialog.Show("Moves With Grid", column.get_Parameter(BuiltInParameter.INSTANCE_MOVES_WITH_GRID_PARAM).AsValueString());
                    break;
                case "Room Bounding":
                    TaskDialog.Show("Room Bounding", column.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).AsValueString());
                    break;
                case "Comments":
                    TaskDialog.Show("Comments", column.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString());
                    break;
                case "Material":
                    foreach (Parameter parameter in column.Symbol.Parameters)
                    {
                        if (parameter.Definition.Name == "Material") // 材质
                        {
                            Material material = column.Document.GetElement(parameter.AsElementId()) as Material;
                            TaskDialog.Show("Material", material.Name);
                        }
                    }
                    break;
                case "Color":
                    foreach (Parameter parameter in column.Symbol.Parameters)
                    {
                        if (parameter.Definition.Name == "Material") // 材质
                        {
                            Material material = column.Document.GetElement(parameter.AsElementId()) as Material;
                            TaskDialog.Show("Color (RGB)", material.Color.Red.ToString() + ", " + material.Color.Green.ToString() + ", " + material.Color.Blue.ToString());
                        }
                    }
                    break;
                case "Depth":
                    foreach (Parameter parameter in column.Symbol.Parameters)
                    {
                        if (parameter.Definition.Name == "Depth") // 深度
                        {
                            TaskDialog.Show("Depth", parameter.AsValueString());
                        }
                    }
                    break;
                case "Offset Base":
                    foreach (Parameter parameter in column.Symbol.Parameters)
                    {
                        if (parameter.Definition.Name == "Offset Base") // 偏移基准
                        {
                            TaskDialog.Show("Offset Base", parameter.AsValueString());
                        }
                    }
                    break;
                case "Offset Top":
                    foreach (Parameter parameter in column.Symbol.Parameters)
                    {
                        if (parameter.Definition.Name == "Offset Top") // 偏移顶部
                        {
                            TaskDialog.Show("Offset Top", parameter.AsValueString());
                        }
                    }
                    break;
                case "Width":
                    foreach (Parameter parameter in column.Symbol.Parameters)
                    {
                        if (parameter.Definition.Name == "Width") // 宽度
                        {
                            TaskDialog.Show("Width", parameter.AsValueString());
                        }
                    }
                    break;
                case "Type Comments":
                    TaskDialog.Show("Type Comments", column.Symbol.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS).AsString());
                    break;
                case "Coordinate":
                    BoundingBoxXYZ bounding = column.get_BoundingBox(column.Document.ActiveView);
                    TaskDialog.Show("Coordinate", "Max: " + bounding.Max.ToString() + "\nMin: " + bounding.Min.ToString());
                    break;
            }
        }

        // deletion
        public static void Delete(FamilyInstance column)
        {
            using (Transaction tr = new Transaction(column.Document, "delete_column"))
            {
                tr.Start();
                column.Document.Delete(column.Id);
                tr.Commit();
            }
        }

        // creation
        public static void Create(Document doc, XYZ p1, Dictionary<string, string> dic)
        {
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
            FamilySymbol symbol = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .WhereElementIsElementType()
                .Cast<FamilySymbol>()
                .FirstOrDefault(x => x.FamilyName == "Columns_Rectangular");

            FamilyInstance column = null;
            // create the column
            using (Transaction tr = new Transaction(doc, "create_column"))
            {
                tr.Start();
                if (!symbol.IsActive)
                {
                    symbol.Activate();
                }
                column = doc.Create.NewFamilyInstance(p1, symbol, level, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                tr.Commit();
            }
            // modify based on the user arguments
            if (dic.ContainsKey("Base Level"))
            {
                ModifyBaseLevel(column, dic["Base Level"]);
            }
            if (dic.ContainsKey("Base Offset"))
            {
                ModifyBaseOffset(column, double.Parse(dic["Base Offset"]));
            }
            if (dic.ContainsKey("Top Level"))
            {
                ModifyTopLevel(column, dic["Top Level"]);
            }
            if (dic.ContainsKey("Top Offset"))
            {
                ModifyTopOffset(column, double.Parse(dic["Top Offset"]));
            }
            if (dic.ContainsKey("Moves With Grid"))
            {
                ModifyMovesWithGrid(column, int.Parse(dic["Moves With Grid"]));
            }
            if (dic.ContainsKey("Room Bounding"))
            {
                ModifyRoomBounding(column, int.Parse(dic["Room Bounding"]));
            }
            if (dic.ContainsKey("Comments"))
            {
                ModifyComments(column, dic["Comments"]);
            }
            if (dic.ContainsKey("Material"))
            {
                ModifyMaterial(column, dic["Material"]);
            }
            if (dic.ContainsKey("Color"))
            {
                ModifyColor(column, dic["Color"]);
            }
            if (dic.ContainsKey("Depth"))
            {
                ModifyDepth(column, double.Parse(dic["Depth"]));
            }
            if (dic.ContainsKey("Offset Base"))
            {
                ModifyOffsetBase(column, double.Parse(dic["Offset Base"]));
            }
            if (dic.ContainsKey("Offset Top"))
            {
                ModifyOffsetTop(column, double.Parse(dic["Offset Top"]));
            }
            if (dic.ContainsKey("Width"))
            {
                ModifyWidth(column, double.Parse(dic["Width"]));
            }
            if (dic.ContainsKey("Type Comments"))
            {
                ModifyTypeComments(column, dic["Type Comments"]);
            }
        }
    }
}
