using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIM
{
    internal class WindowFunc
    {
        // modification
        public static void Modify(FamilyInstance window, string property_name, string property_value)
        {
            switch (property_name)
            {
                case "Level":
                    ModifyLevel(window, property_value);
                    break;
                case "Sill Height":
                    ModifySillHeight(window, double.Parse(property_value));
                    break;
                case "Orientation":
                    ModifyOrientation(window, int.Parse(property_value));
                    break;
                case "Head Height":
                    ModifyHeadHeight(window, double.Parse(property_value));
                    break;
                case "Comments":
                    ModifyComments(window, property_value);
                    break;
                case "Wall Closure":
                    ModifyWallClosure(window, int.Parse(property_value));
                    break;
                case "Glass Pane Material":
                    ModifyGlassPaneMaterial(window, property_value);
                    break;
                case "Glass Pane Color":
                    ModifyGlassPaneColor(window, property_value);
                    break;
                case "Sash Material":
                    ModifySashMaterial(window, property_value);
                    break;
                case "Sash Color":
                    ModifySashColor(window, property_value);
                    break;
                case "Height":
                    ModifyHeight(window, double.Parse(property_value));
                    break;
                case "Width":
                    ModifyWidth(window, double.Parse(property_value));
                    break;
                case "Window Inset":
                    ModifyWindowInset(window, double.Parse(property_value));
                    break;
                case "Type Comments":
                    ModifyTypeComments(window, property_value);
                    break;
            }
        }

        // instance properties
        public static void ModifyLevel(FamilyInstance window, string level_str)
        {
            // find the level id with the given name
            Level level = new FilteredElementCollector(window.Document)
                .OfClass(typeof(Level))
                .Where(x => x.Name == level_str)
                .Cast<Level>()
                .FirstOrDefault();
            // modify the level of the window
            using (Transaction tr = new Transaction(window.Document, "modify_window_level"))
            {
                tr.Start();
                window.get_Parameter(BuiltInParameter.FAMILY_LEVEL_PARAM).Set(level.Id);
                tr.Commit();
            }
        }

        public static void ModifySillHeight(FamilyInstance window, double height)
        {
            // modify the sill height of the window
            // the metric of offest here is feet
            using (Transaction tr = new Transaction(window.Document, "modify_window_sill_height"))
            {
                tr.Start();
                window.get_Parameter(BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM).Set(height);
                tr.Commit();
            }
        }

        public static void ModifyOrientation(FamilyInstance window, int ori)
        {
            // modify the orientation property
            // 0->Vertical, 1->Slanted
            using (Transaction tr = new Transaction(window.Document, "modify_window_orientation"))
            {
                tr.Start();
                window.get_Parameter(BuiltInParameter.INSERT_ORIENTATION).Set(ori);
                tr.Commit();
            }
        }

        public static void ModifyHeadHeight(FamilyInstance window, double height)
        {
            // modify the head height of the window
            // the metric of offest here is feet
            using (Transaction tr = new Transaction(window.Document, "modify_window_head_height"))
            {
                tr.Start();
                window.get_Parameter(BuiltInParameter.INSTANCE_HEAD_HEIGHT_PARAM).Set(height);
                tr.Commit();
            }
        }

        public static void ModifyComments(FamilyInstance window, string comment)
        {
            // modify the comments of the window
            using (Transaction tr = new Transaction(window.Document, "modify_window_comments"))
            {
                tr.Start();
                window.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set(comment);
                tr.Commit();
            }
        }

        // type properties
        public static void ModifyWallClosure(FamilyInstance window, int closure)
        {
            // modify the wall closure property
            // 0->By host, 1->Neither, 2->Interior, 3->Exterior, 4->Both
            // duplicate the type
            FamilySymbol familySymbol = window.Symbol;
            string origin_name = familySymbol.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(window.Document, "modify_window_wall_closure"))
            {
                tr.Start();
                ElementId new_type_id = familySymbol.Duplicate(origin_name + "_wc_option_" + closure.ToString()).Id;
                window.Document.GetElement(new_type_id).get_Parameter(BuiltInParameter.TYPE_WALL_CLOSURE).Set(closure);
                window.Symbol = window.Document.GetElement(new_type_id) as FamilySymbol;
                tr.Commit();
            }
        }

        public static void ModifyGlassPaneMaterial(FamilyInstance window, string material_name)
        {
            // modify the material of the glass pane
            // duplicate the symbol of the window
            FamilySymbol familySymbol = window.Symbol;
            string origin_name = familySymbol.Name;
            // get all the mateirals
            FilteredElementCollector collector = new FilteredElementCollector(window.Document);
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
            using (Transaction tr = new Transaction(window.Document, "modify_window_glass_pane_material"))
            {
                tr.Start();
                ElementId new_type_id = familySymbol.Duplicate(origin_name + "_pane_material_" + material_name).Id;
                FamilySymbol newSymbol = window.Document.GetElement(new_type_id) as FamilySymbol;

                foreach (Parameter parameter in newSymbol.Parameters)
                {
                    if (parameter.Definition.Name == "Glass Pane Material") // 玻璃嵌板材质
                    {
                        // Material sashMaterial = window.Document.GetElement(parameter.AsElementId()) as Material;
                        Material sashMaterial = window.Document.GetElement(materialIds[min_index]) as Material;
                        parameter.Set(sashMaterial.Id);
                        window.Symbol = newSymbol;
                    }
                }

                tr.Commit();
            }
        }

        public static void ModifyGlassPaneColor(FamilyInstance window, string color)
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
            // duplicate the symbol of the window
            FamilySymbol familySymbol = window.Symbol;
            string origin_name = familySymbol.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(window.Document, "modify_window_glass_pane_color"))
            {
                tr.Start();
                ElementId new_type_id = familySymbol.Duplicate(origin_name + "_pane_color_" + color).Id;
                FamilySymbol newSymbol = window.Document.GetElement(new_type_id) as FamilySymbol;

                foreach (Parameter parameter in newSymbol.Parameters)
                {
                    if (parameter.Definition.Name == "Glass Material") // 玻璃嵌板材质
                    {
                        Material sashMaterial = window.Document.GetElement(parameter.AsElementId()) as Material;
                        sashMaterial = sashMaterial.Duplicate(sashMaterial.Name + "_pane_color_" + color);
                        sashMaterial.Color = color_map[color];
                        parameter.Set(sashMaterial.Id);
                        window.Symbol = newSymbol;
                    }
                }

                tr.Commit();
            }
        }

        public static void ModifySashMaterial(FamilyInstance window, string material_name)
        {
            // modify the material of the sash
            // duplicate the symbol of the window
            FamilySymbol familySymbol = window.Symbol;
            string origin_name = familySymbol.Name;
            // get all the mateirals
            FilteredElementCollector collector = new FilteredElementCollector(window.Document);
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
            using (Transaction tr = new Transaction(window.Document, "modify_window_sash_material"))
            {
                tr.Start();
                ElementId new_type_id = familySymbol.Duplicate(origin_name + "_sash_material_" + material_name).Id;
                FamilySymbol newSymbol = window.Document.GetElement(new_type_id) as FamilySymbol;

                foreach (Parameter parameter in newSymbol.Parameters)
                {
                    if (parameter.Definition.Name == "Sash Material") // 窗扇材质
                    {
                        // Material sashMaterial = window.Document.GetElement(parameter.AsElementId()) as Material;
                        Material sashMaterial = window.Document.GetElement(materialIds[min_index]) as Material;
                        parameter.Set(sashMaterial.Id);
                        window.Symbol = newSymbol;
                    }
                }

                tr.Commit();
            }
        }

        public static void ModifySashColor(FamilyInstance window, string color)
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
            // duplicate the symbol of the window
            FamilySymbol familySymbol = window.Symbol;
            string origin_name = familySymbol.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(window.Document, "modify_window_sash_color"))
            {
                tr.Start();
                ElementId new_type_id = familySymbol.Duplicate(origin_name + "_sash_color_" + color).Id;
                FamilySymbol newSymbol = window.Document.GetElement(new_type_id) as FamilySymbol;

                foreach (Parameter parameter in newSymbol.Parameters)
                {
                    if (parameter.Definition.Name == "Sash Material") // 窗扇材质
                    {
                        Material sashMaterial = window.Document.GetElement(parameter.AsElementId()) as Material;
                        sashMaterial = sashMaterial.Duplicate(sashMaterial.Name + "_sash_color_" + color);
                        sashMaterial.Color = color_map[color];
                        parameter.Set(sashMaterial.Id);
                        window.Symbol = newSymbol;
                    }
                }

                tr.Commit();
            }
        }

        public static void ModifyHeight(FamilyInstance window, double height)
        {
            // modify the height of the window
            // the metric here is feet
            // duplicate the type
            FamilySymbol familySymbol = window.Symbol;
            string origin_name = familySymbol.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(window.Document, "modify_window_height"))
            {
                tr.Start();
                ElementId new_type_id = familySymbol.Duplicate(origin_name + "_height_" + height.ToString()).Id;
                window.Document.GetElement(new_type_id).get_Parameter(BuiltInParameter.DOOR_HEIGHT).Set(height);
                window.Symbol = window.Document.GetElement(new_type_id) as FamilySymbol;
                tr.Commit();
            }
        }

        public static void ModifyWidth(FamilyInstance window, double width)
        {
            // modify the width of the window
            // the metric here is feet
            // duplicate the type
            FamilySymbol familySymbol = window.Symbol;
            string origin_name = familySymbol.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(window.Document, "modify_window_width"))
            {
                tr.Start();
                ElementId new_type_id = familySymbol.Duplicate(origin_name + "_width_" + width.ToString()).Id;
                window.Document.GetElement(new_type_id).get_Parameter(BuiltInParameter.FURNITURE_WIDTH).Set(width);
                window.Symbol = window.Document.GetElement(new_type_id) as FamilySymbol;
                tr.Commit();
            }
        }

        public static void ModifyWindowInset(FamilyInstance window, double inset)
        {
            // modify the window inset of the window
            // the metric here is feet
            // duplicate the type
            FamilySymbol familySymbol = window.Symbol;
            string origin_name = familySymbol.Name;

            // modify the property of the new type
            using (Transaction tr = new Transaction(window.Document, "modify_window_inset"))
            {
                tr.Start();
                ElementId new_type_id = familySymbol.Duplicate(origin_name + "_inset_" + inset.ToString()).Id;
                FamilySymbol newSymbol = window.Document.GetElement(new_type_id) as FamilySymbol;

                foreach (Parameter parameter in newSymbol.Parameters)
                {
                    if (parameter.Definition.Name == "Window Inset") // 窗嵌入
                    {
                        parameter.Set(inset);
                        window.Symbol = newSymbol;
                    }
                }

                tr.Commit();
            }
        }

        public static void ModifyTypeComments(FamilyInstance window, string comment)
        {
            // modify the comments of the window type
            // duplicate the type
            FamilySymbol familySymbol = window.Symbol;
            string origin_name = familySymbol.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(window.Document, "modify_window_type_comments"))
            {
                tr.Start();
                ElementId new_type_id = familySymbol.Duplicate(origin_name + "_type_comments_" + comment).Id;
                window.Document.GetElement(new_type_id).get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS).Set(comment);
                window.Symbol = window.Document.GetElement(new_type_id) as FamilySymbol;
                tr.Commit();
            }
        }

        // retrieval
        public static void Retrieval(FamilyInstance window, string option)
        {
            // retrieve corresponding properties of the window and display in the Revit UI
            switch (option)
            {
                case "Level":
                    TaskDialog.Show("Level", window.get_Parameter(BuiltInParameter.FAMILY_LEVEL_PARAM).AsValueString());
                    break;
                case "Sill Height":
                    TaskDialog.Show("Sill Height", window.get_Parameter(BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM).AsValueString());
                    break;
                case "Orientation":
                    TaskDialog.Show("Orientation", window.get_Parameter(BuiltInParameter.INSERT_ORIENTATION).AsValueString());
                    break;
                case "Head Height":
                    TaskDialog.Show("Head Height", window.get_Parameter(BuiltInParameter.INSTANCE_HEAD_HEIGHT_PARAM).AsValueString());
                    break;
                case "Comments":
                    TaskDialog.Show("Comments", window.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsValueString());
                    break;
                case "Wall Closure":
                    TaskDialog.Show("Wall Closure", window.Symbol.get_Parameter(BuiltInParameter.TYPE_WALL_CLOSURE).AsValueString());
                    break;
                case "Glass Pane Material":
                    foreach (Parameter parameter in window.Symbol.Parameters)
                    {
                        if (parameter.Definition.Name == "Glass Pane Material") // 玻璃嵌板材质
                        {
                            Material material = window.Document.GetElement(parameter.AsElementId()) as Material;
                            TaskDialog.Show("Glass Pane Material", material.Name);
                        }
                    }
                    break;
                case "Glass Pane Color":
                    foreach (Parameter parameter in window.Symbol.Parameters)
                    {
                        if (parameter.Definition.Name == "Glass Pane Material") // 玻璃嵌板材质
                        {
                            Material material = window.Document.GetElement(parameter.AsElementId()) as Material;
                            TaskDialog.Show("Glass Pane Color (RGB)", material.Color.Red.ToString() + ", " + material.Color.Green.ToString() + ", " + material.Color.Blue.ToString());
                        }
                    }
                    break;
                case "Sash Material":
                    foreach (Parameter parameter in window.Symbol.Parameters)
                    {
                        if (parameter.Definition.Name == "Sash Material") // 窗扇
                        {
                            Material material = window.Document.GetElement(parameter.AsElementId()) as Material;
                            TaskDialog.Show("Sash Material", material.Name);
                        }
                    }
                    break;
                case "Sash Color":
                    foreach (Parameter parameter in window.Symbol.Parameters)
                    {
                        if (parameter.Definition.Name == "Sash Material") // 窗扇
                        {
                            Material material = window.Document.GetElement(parameter.AsElementId()) as Material;
                            TaskDialog.Show("Sash Color (RGB)", material.Color.Red.ToString() + ", " + material.Color.Green.ToString() + ", " + material.Color.Blue.ToString());
                        }
                    }
                    break;
                case "Height":
                    TaskDialog.Show("Height", window.Symbol.get_Parameter(BuiltInParameter.DOOR_HEIGHT).AsValueString());
                    break;
                case "Width":
                    TaskDialog.Show("Width", window.Symbol.get_Parameter(BuiltInParameter.FURNITURE_WIDTH).AsValueString());
                    break;
                case "Inset":
                    foreach (Parameter parameter in window.Symbol.Parameters)
                    {
                        if (parameter.Definition.Name == "Window Inset") // 窗嵌入
                        {
                            TaskDialog.Show("Inset", parameter.AsValueString());
                        }
                    }
                    break;
                case "Type Comments":
                    TaskDialog.Show("Type Comments", window.Symbol.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS).AsValueString());
                    break;
                case "Coordinate":
                    BoundingBoxXYZ bounding = window.get_BoundingBox(window.Document.ActiveView);
                    TaskDialog.Show("Coordinate", "Max: " + bounding.Max.ToString() + "\nMin: " + bounding.Min.ToString());
                    break;
            }
        }

        // deletion
        public static void Delete(FamilyInstance window)
        {
            using (Transaction tr = new Transaction(window.Document, "delete_window"))
            {
                tr.Start();
                window.Document.Delete(window.Id);
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
                .FirstOrDefault(x => x.FamilyName == "Windows_Sgl_Plain"); // optional
            // get the nearest wall near the point p1
            Wall wall = new FilteredElementCollector(doc)
                .OfClass(typeof(Wall))
                .Where(x => x.Location != null)
                .Cast<Wall>()
                .OrderBy(x => (x.Location as LocationCurve).Curve.Distance(p1))
                .FirstOrDefault();

            FamilyInstance window = null;
            // create the window
            using (Transaction tr = new Transaction(doc, "create_window"))
            {
                tr.Start();
                if (!symbol.IsActive)
                {
                    symbol.Activate();
                }
                window = doc.Create.NewFamilyInstance(p1, symbol, wall, level, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                tr.Commit();
            }
            // modify based on the user arguments
            if (dic.ContainsKey("Sill Height"))
            {
                ModifySillHeight(window, double.Parse(dic["Sill Height"]));
            }
            if (dic.ContainsKey("Orientation"))
            {
                ModifyOrientation(window, int.Parse(dic["Orientation"]));
            }
            if (dic.ContainsKey("Head Height"))
            {
                ModifyHeadHeight(window, double.Parse(dic["Head Height"]));
            }
            if (dic.ContainsKey("Comments"))
            {
                ModifyComments(window, dic["Comments"]);
            }
            if (dic.ContainsKey("Wall Closure"))
            {
                ModifyWallClosure(window, int.Parse(dic["Wall Closure"]));
            }
            if (dic.ContainsKey("Glass Pane Material"))
            {
                ModifyGlassPaneMaterial(window, dic["Glass Pane Material"]);
            }
            if (dic.ContainsKey("Glass Pane Color"))
            {
                ModifyGlassPaneColor(window, dic["Glass Pane Color"]);
            }
            if (dic.ContainsKey("Sash Material"))
            {
                ModifySashMaterial(window, dic["Sash Material"]);
            }
            if (dic.ContainsKey("Sash Color"))
            {
                ModifySashColor(window, dic["Sash Color"]);
            }
            if (dic.ContainsKey("Height"))
            {
                ModifyHeight(window, double.Parse(dic["Height"]));
            }
            if (dic.ContainsKey("Width"))
            {
                ModifyWidth(window, double.Parse(dic["Width"]));
            }
            if (dic.ContainsKey("Window Inset"))
            {
                ModifyWindowInset(window, double.Parse(dic["Inset"]));
            }
            if (dic.ContainsKey("Type Comments"))
            {
                ModifyTypeComments(window, dic["Type Comments"]);
            }
        }
    }
}
