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
    internal class DoorFunc
    {
        // modification
        public static void Modify(FamilyInstance door, string property_name, string property_value)
        {
            switch (property_name)
            {
                case "Level":
                    ModifyLevel(door, property_value);
                    break;
                case "Sill Height":
                    ModifySillHeight(door, double.Parse(property_value));
                    break;
                case "Orientation":
                    ModifyOrientation(door, int.Parse(property_value));
                    break;
                case "Head Height":
                    ModifyHeadHeight(door, double.Parse(property_value));
                    break;
                case "Comments":
                    ModifyComments(door, property_value);
                    break;
                case "Wall Closure":
                    ModifyWallClosure(door, int.Parse(property_value));
                    break;
                case "Function":
                    ModifyFunction(door, int.Parse(property_value));
                    break;
                case "Door Material":
                    ModifyDoorMaterial(door, property_value);
                    break;
                case "Door Color":
                    ModifyDoorColor(door, property_value);
                    break;
                case "Frame Material":
                    ModifyFrameMaterial(door, property_value);
                    break;
                case "Frame Color":
                    ModifyFrameColor(door, property_value);
                    break;
                case "Thickness":
                    ModifyThickness(door, double.Parse(property_value));
                    break;
                case "Height":
                    ModifyHeight(door, double.Parse(property_value));
                    break;
                case "Trim Projection Exterior":
                    ModifyTrimProjectionExterior(door, double.Parse(property_value));
                    break;
                case "Trim Projection Interior":
                    ModifyTrimProjectionInterior(door, double.Parse(property_value));
                    break;
                case "Trim Width":
                    ModifyTrimWidth(door, double.Parse(property_value));
                    break;
                case "Width":
                    ModifyWidth(door, double.Parse(property_value));
                    break;
                case "Type Comments":
                    ModifyTypeComments(door, property_value);
                    break;
            }
        }

        // instance properties
        public static void ModifyLevel(FamilyInstance door, string level_str)
        {
            // find the level id with the given name
            Level level = new FilteredElementCollector(door.Document)
                .OfClass(typeof(Level))
                .Where(x => x.Name == level_str)
                .Cast<Level>()
                .FirstOrDefault();
            // modify the level of the door
            using (Transaction tr = new Transaction(door.Document, "modify_door_level"))
            {
                tr.Start();
                door.get_Parameter(BuiltInParameter.FAMILY_LEVEL_PARAM).Set(level.Id);
                tr.Commit();
            }
        }

        public static void ModifySillHeight(FamilyInstance door, double height)
        {
            // modify the sill height of the door
            // the metric of offest here is feet
            using (Transaction tr = new Transaction(door.Document, "modify_door_sill_height"))
            {
                tr.Start();
                door.get_Parameter(BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM).Set(height);
                tr.Commit();
            }
        }

        public static void ModifyOrientation(FamilyInstance door, int ori)
        {
            // modify the orientation property
            // 0->Vertical, 1->Slanted
            using (Transaction tr = new Transaction(door.Document, "modify_door_orientation"))
            {
                tr.Start();
                door.get_Parameter(BuiltInParameter.INSERT_ORIENTATION).Set(ori);
                tr.Commit();
            }
        }

        public static void ModifyHeadHeight(FamilyInstance door, double height)
        {
            // modify the head height of the door
            // the metric of offest here is feet
            using (Transaction tr = new Transaction(door.Document, "modify_door_head_height"))
            {
                tr.Start();
                door.get_Parameter(BuiltInParameter.INSTANCE_HEAD_HEIGHT_PARAM).Set(height);
                tr.Commit();
            }
        }

        public static void ModifyComments(FamilyInstance door, string comment)
        {
            // modify the comments of the door
            using (Transaction tr = new Transaction(door.Document, "modify_door_comments"))
            {
                tr.Start();
                door.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set(comment);
                tr.Commit();
            }
        }

        // type properties
        public static void ModifyWallClosure(FamilyInstance door, int closure)
        {
            // modify the wall closure property
            // 0->By host, 1->Neither, 2->Interior, 3->Exterior, 4->Both
            // duplicate the type
            FamilySymbol familySymbol = door.Symbol;
            string origin_name = familySymbol.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(door.Document, "modify_door_wall_closure"))
            {
                tr.Start();
                ElementId new_type_id = familySymbol.Duplicate(origin_name + "_wc_option_" + closure.ToString()).Id;
                door.Document.GetElement(new_type_id).get_Parameter(BuiltInParameter.TYPE_WALL_CLOSURE).Set(closure);
                door.Symbol = door.Document.GetElement(new_type_id) as FamilySymbol;
                tr.Commit();
            }
        }

        public static void ModifyFunction(FamilyInstance door, int func)
        {
            // modify the function property
            // 0->Interior, 1->Exterior
            // duplicate the type
            FamilySymbol familySymbol = door.Symbol;
            string origin_name = familySymbol.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(door.Document, "modify_door_function"))
            {
                tr.Start();
                ElementId new_type_id = familySymbol.Duplicate(origin_name + "_func_option_" + func.ToString()).Id;
                door.Document.GetElement(new_type_id).get_Parameter(BuiltInParameter.FUNCTION_PARAM).Set(func);
                door.Symbol = door.Document.GetElement(new_type_id) as FamilySymbol;
                tr.Commit();
            }
        }

        public static void ModifyDoorMaterial(FamilyInstance door, string material_name)
        {
            // modify the material of the door
            // duplicate the symbol of the door
            FamilySymbol familySymbol = door.Symbol;
            string origin_name = familySymbol.Name;
            // get all the mateirals
            FilteredElementCollector collector = new FilteredElementCollector(door.Document);
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
            using (Transaction tr = new Transaction(door.Document, "modify_door_material"))
            {
                tr.Start();
                ElementId new_type_id = familySymbol.Duplicate(origin_name + "_door_material_" + material_name).Id;
                FamilySymbol newSymbol = door.Document.GetElement(new_type_id) as FamilySymbol;

                foreach (Parameter parameter in newSymbol.Parameters)
                {
                    if (parameter.Definition.Name == "Material Door") // 门材质
                    {
                        Material doorMaterial = door.Document.GetElement(materialIds[min_index]) as Material;
                        parameter.Set(doorMaterial.Id);
                        door.Symbol = newSymbol;
                    }
                }

                tr.Commit();
            }
        }

        public static void ModifyDoorColor(FamilyInstance door, string color)
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
            // duplicate the symbol of the door
            FamilySymbol familySymbol = door.Symbol;
            string origin_name = familySymbol.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(door.Document, "modify_door_color"))
            {
                tr.Start();
                ElementId new_type_id = familySymbol.Duplicate(origin_name + "_door_color_" + color).Id;
                FamilySymbol newSymbol = door.Document.GetElement(new_type_id) as FamilySymbol;

                foreach (Parameter parameter in newSymbol.Parameters)
                {
                    if (parameter.Definition.Name == "Material Door") // 门材质
                    {
                        Material doorMaterial = door.Document.GetElement(parameter.AsElementId()) as Material;
                        doorMaterial = doorMaterial.Duplicate(doorMaterial.Name + "_door_color_" + color);
                        doorMaterial.Color = color_map[color];
                        parameter.Set(doorMaterial.Id);
                        door.Symbol = newSymbol;
                    }
                }

                tr.Commit();
            }
        }

        public static void ModifyFrameMaterial(FamilyInstance door, string material_name)
        {
            // modify the material of the frame
            // duplicate the symbol of the door
            FamilySymbol familySymbol = door.Symbol;
            string origin_name = familySymbol.Name;
            // get all the mateirals
            FilteredElementCollector collector = new FilteredElementCollector(door.Document);
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
            using (Transaction tr = new Transaction(door.Document, "modify_door_frame_material"))
            {
                tr.Start();
                ElementId new_type_id = familySymbol.Duplicate(origin_name + "_frame_material_" + material_name).Id;
                FamilySymbol newSymbol = door.Document.GetElement(new_type_id) as FamilySymbol;

                foreach (Parameter parameter in newSymbol.Parameters)
                {
                    if (parameter.Definition.Name == "Material Frame") // 框架材质
                    {
                        Material FrameMaterial = door.Document.GetElement(materialIds[min_index]) as Material;
                        parameter.Set(FrameMaterial.Id);
                        door.Symbol = newSymbol;
                    }
                }

                tr.Commit();
            }
        }

        public static void ModifyFrameColor(FamilyInstance door, string color)
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
            // duplicate the symbol of the door
            FamilySymbol familySymbol = door.Symbol;
            string origin_name = familySymbol.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(door.Document, "modify_door_frame_color"))
            {
                tr.Start();
                ElementId new_type_id = familySymbol.Duplicate(origin_name + "_frame_color_" + color).Id;
                FamilySymbol newSymbol = door.Document.GetElement(new_type_id) as FamilySymbol;

                foreach (Parameter parameter in newSymbol.Parameters)
                {
                    if (parameter.Definition.Name == "Material Frame") // 框架材质
                    {
                        Material frameMaterial = door.Document.GetElement(parameter.AsElementId()) as Material;
                        frameMaterial = frameMaterial.Duplicate(frameMaterial.Name + "_frame_color_" + color);
                        frameMaterial.Color = color_map[color];
                        parameter.Set(frameMaterial.Id);
                        door.Symbol = newSymbol;
                    }
                }

                tr.Commit();
            }
        }

        public static void ModifyThickness(FamilyInstance door, double thickness)
        {
            // modify the thickness of the door
            // the metric here is feet
            // duplicate the type
            FamilySymbol familySymbol = door.Symbol;
            string origin_name = familySymbol.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(door.Document, "modify_door_thickness"))
            {
                tr.Start();
                ElementId new_type_id = familySymbol.Duplicate(origin_name + "_thickness_" + thickness.ToString()).Id;
                door.Document.GetElement(new_type_id).get_Parameter(BuiltInParameter.GENERIC_THICKNESS).Set(thickness);
                door.Symbol = door.Document.GetElement(new_type_id) as FamilySymbol;
                tr.Commit();
            }
        }

        public static void ModifyHeight(FamilyInstance door, double height)
        {
            // modify the height of the door
            // the metric here is feet
            // duplicate the type
            FamilySymbol familySymbol = door.Symbol;
            string origin_name = familySymbol.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(door.Document, "modify_door_height"))
            {
                tr.Start();
                ElementId new_type_id = familySymbol.Duplicate(origin_name + "_height_" + height.ToString()).Id;
                door.Document.GetElement(new_type_id).get_Parameter(BuiltInParameter.DOOR_HEIGHT).Set(height);
                door.Symbol = door.Document.GetElement(new_type_id) as FamilySymbol;
                tr.Commit();
            }
        }

        public static void ModifyTrimProjectionExterior(FamilyInstance door, double offset)
        {
            // modify the Trim Projection Exterior of the door
            // the metric here is feet
            // duplicate the type
            FamilySymbol familySymbol = door.Symbol;
            string origin_name = familySymbol.Name;

            // modify the property of the new type
            using (Transaction tr = new Transaction(door.Document, "modify_door_trim_projection_exterior"))
            {
                tr.Start();
                ElementId new_type_id = familySymbol.Duplicate(origin_name + "_trim_projection_exterior_" + offset.ToString()).Id;
                FamilySymbol newSymbol = door.Document.GetElement(new_type_id) as FamilySymbol;

                foreach (Parameter parameter in newSymbol.Parameters)
                {
                    if (parameter.Definition.Name == "Trim Projection Exterior") // 贴面投影外部
                    {
                        parameter.Set(offset);
                        door.Symbol = newSymbol;
                    }
                }

                tr.Commit();
            }
        }

        public static void ModifyTrimProjectionInterior(FamilyInstance door, double offset)
        {
            // modify the Trim Projection Interior of the door
            // the metric here is feet
            // duplicate the type
            FamilySymbol familySymbol = door.Symbol;
            string origin_name = familySymbol.Name;

            // modify the property of the new type
            using (Transaction tr = new Transaction(door.Document, "modify_door_trim_projection_interior"))
            {
                tr.Start();
                ElementId new_type_id = familySymbol.Duplicate(origin_name + "_trim_projection_interior_" + offset.ToString()).Id;
                FamilySymbol newSymbol = door.Document.GetElement(new_type_id) as FamilySymbol;

                foreach (Parameter parameter in newSymbol.Parameters)
                {
                    if (parameter.Definition.Name == "Trim Projection Interior") // 贴面投影内部
                    {
                        parameter.Set(offset);
                        door.Symbol = newSymbol;
                    }
                }

                tr.Commit();
            }
        }

        public static void ModifyTrimWidth(FamilyInstance door, double offset)
        {
            // modify the Trim Width of the door
            // the metric here is feet
            // duplicate the type
            FamilySymbol familySymbol = door.Symbol;
            string origin_name = familySymbol.Name;

            // modify the property of the new type
            using (Transaction tr = new Transaction(door.Document, "modify_door_trim_width"))
            {
                tr.Start();
                ElementId new_type_id = familySymbol.Duplicate(origin_name + "_trim_width_" + offset.ToString()).Id;
                FamilySymbol newSymbol = door.Document.GetElement(new_type_id) as FamilySymbol;

                foreach (Parameter parameter in newSymbol.Parameters)
                {
                    if (parameter.Definition.Name == "Trim Width") // 贴面宽度
                    {
                        parameter.Set(offset);
                        door.Symbol = newSymbol;
                    }
                }

                tr.Commit();
            }
        }

        public static void ModifyWidth(FamilyInstance door, double width)
        {
            // modify the width of the door
            // the metric here is feet
            // duplicate the type
            FamilySymbol familySymbol = door.Symbol;
            string origin_name = familySymbol.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(door.Document, "modify_door_width"))
            {
                tr.Start();
                ElementId new_type_id = familySymbol.Duplicate(origin_name + "_width_" + width.ToString()).Id;
                door.Document.GetElement(new_type_id).get_Parameter(BuiltInParameter.FURNITURE_WIDTH).Set(width);
                door.Symbol = door.Document.GetElement(new_type_id) as FamilySymbol;
                tr.Commit();
            }
        }

        public static void ModifyTypeComments(FamilyInstance door, string comment)
        {
            // modify the comments of the door type
            // duplicate the type
            FamilySymbol familySymbol = door.Symbol;
            string origin_name = familySymbol.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(door.Document, "modify_door_type_comments"))
            {
                tr.Start();
                ElementId new_type_id = familySymbol.Duplicate(origin_name + "_type_comments_" + comment).Id;
                door.Document.GetElement(new_type_id).get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS).Set(comment);
                door.Symbol = door.Document.GetElement(new_type_id) as FamilySymbol;
                tr.Commit();
            }
        }

        // retrieval
        public static void Retrieval(FamilyInstance door, string option)
        {
            // retrieve corresponding properties of the door and display in the Revit UI
            switch (option)
            {
                case "Level":
                    TaskDialog.Show("Level", door.get_Parameter(BuiltInParameter.FAMILY_LEVEL_PARAM).AsValueString());
                    break;
                case "Sill Height":
                    TaskDialog.Show("Sill Height", door.get_Parameter(BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM).AsValueString());
                    break;
                case "Orientation":
                    TaskDialog.Show("Orientation", door.get_Parameter(BuiltInParameter.INSERT_ORIENTATION).AsValueString());
                    break;
                case "Head Height":
                    TaskDialog.Show("Head Height", door.get_Parameter(BuiltInParameter.INSTANCE_HEAD_HEIGHT_PARAM).AsValueString());
                    break;
                case "Comments":
                    TaskDialog.Show("Comments", door.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString());
                    break;
                case "Wall Closure":
                    TaskDialog.Show("Wall Closure", door.Symbol.get_Parameter(BuiltInParameter.TYPE_WALL_CLOSURE).AsValueString());
                    break;
                case "Function":
                    TaskDialog.Show("Function", door.Symbol.get_Parameter(BuiltInParameter.FUNCTION_PARAM).AsValueString());
                    break;
                case "Door Material":
                    foreach (Parameter parameter in door.Symbol.Parameters)
                    {
                        if (parameter.Definition.Name == "Door Material") // 门材质
                        {
                            Material material = door.Document.GetElement(parameter.AsElementId()) as Material;
                            TaskDialog.Show("Door Material", material.Name);
                        }
                    }
                    break;
                case "Door Color":
                    foreach (Parameter parameter in door.Symbol.Parameters)
                    {
                        if (parameter.Definition.Name == "Door Material") // 门材质
                        {
                            Material material = door.Document.GetElement(parameter.AsElementId()) as Material;
                            TaskDialog.Show("Door Color (RGB)", material.Color.Red.ToString() + ", " + material.Color.Green.ToString() + ", " + material.Color.Blue.ToString());
                        }
                    }
                    break;
                case "Frame Material":
                    foreach (Parameter parameter in door.Symbol.Parameters)
                    {
                        if (parameter.Definition.Name == "Frame Material") // 框架材质
                        {
                            Material material = door.Document.GetElement(parameter.AsElementId()) as Material;
                            TaskDialog.Show("Frame Material", material.Name);
                        }
                    }
                    break;
                case "Frame Color":
                    foreach (Parameter parameter in door.Symbol.Parameters)
                    {
                        if (parameter.Definition.Name == "Frame Material") // 框架材质
                        {
                            Material material = door.Document.GetElement(parameter.AsElementId()) as Material;
                            TaskDialog.Show("Frame Color (RGB)", material.Color.Red.ToString() + ", " + material.Color.Green.ToString() + ", " + material.Color.Blue.ToString());
                        }
                    }
                    break;
                case "Thickness":
                    TaskDialog.Show("Thickness", door.Symbol.get_Parameter(BuiltInParameter.GENERIC_THICKNESS).AsValueString());
                    break;
                case "Height":
                    TaskDialog.Show("Height", door.Symbol.get_Parameter(BuiltInParameter.DOOR_HEIGHT).AsValueString());
                    break;
                case "Trim Projection Exterior":
                    foreach (Parameter parameter in door.Symbol.Parameters)
                    {
                        if (parameter.Definition.Name == "Trim Projection Exterior") // 贴面投影外部
                        {
                            TaskDialog.Show("Trim Projection Exterior", parameter.AsValueString());
                        }
                    }
                    break;
                case "Trim Projection Interior":
                    foreach (Parameter parameter in door.Symbol.Parameters)
                    {
                        if (parameter.Definition.Name == "Trim Projection Interior") // 贴面投影内部
                        {
                            TaskDialog.Show("Trim Projection Interior", parameter.AsValueString());
                        }
                    }
                    break;
                case "Trim Width":
                    foreach (Parameter parameter in door.Symbol.Parameters)
                    {
                        if (parameter.Definition.Name == "Trim Width") // 贴面宽度
                        {
                            TaskDialog.Show("Trim Width", parameter.AsValueString());
                        }
                    }
                    break;
                case "Width":
                    TaskDialog.Show("Width", door.Symbol.get_Parameter(BuiltInParameter.FURNITURE_WIDTH).AsValueString());
                    break;
                case "Type Comments":
                    TaskDialog.Show("Type Comments", door.Symbol.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS).AsString());
                    break;
                case "Coordinate":
                    BoundingBoxXYZ bounding = door.get_BoundingBox(door.Document.ActiveView);
                    TaskDialog.Show("Coordinate", "Max: " + bounding.Max.ToString() + "\nMin: " + bounding.Min.ToString());
                    break;
            }
        }

        // deletion
        public static void Delete(FamilyInstance door)
        {
            using (Transaction tr = new Transaction(door.Document, "delete_door"))
            {
                tr.Start();
                door.Document.Delete(door.Id);
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
                .FirstOrDefault(x => x.FamilyName == "Doors_IntSgl"); // optional
            // get the nearest wall near the point p1
            Wall wall = new FilteredElementCollector(doc)
                .OfClass(typeof(Wall))
                .Where(x => x.Location != null)
                .Cast<Wall>()
                .OrderBy(x => (x.Location as LocationCurve).Curve.Distance(p1))
                .FirstOrDefault();

            FamilyInstance door = null;
            // create the door
            using (Transaction tr = new Transaction(doc, "create_door"))
            {
                tr.Start();
                if (!symbol.IsActive)
                {
                    symbol.Activate();
                }
                door = doc.Create.NewFamilyInstance(p1, symbol, wall, level, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                tr.Commit();
            }
            // modify based on the user arguments
            if (dic.ContainsKey("Sill Height"))
            {
                ModifySillHeight(door, double.Parse(dic["Sill Height"]));
            }
            if (dic.ContainsKey("Orientation"))
            {
                ModifyOrientation(door, int.Parse(dic["Orientation"]));
            }
            if (dic.ContainsKey("Head Height"))
            {
                ModifyHeadHeight(door, double.Parse(dic["Head Height"]));
            }
            if (dic.ContainsKey("Comments"))
            {
                ModifyComments(door, dic["Comments"]);
            }
            if (dic.ContainsKey("Wall Closure"))
            {
                ModifyWallClosure(door, int.Parse(dic["Wall Closure"]));
            }
            if (dic.ContainsKey("Function"))
            {
                ModifyFunction(door, int.Parse(dic["Function"]));
            }
            if (dic.ContainsKey("Door Material"))
            {
                ModifyDoorMaterial(door, dic["Door Material"]);
            }
            if (dic.ContainsKey("Door Color"))
            {
                ModifyDoorColor(door, dic["Door Color"]);
            }
            if (dic.ContainsKey("Frame Material"))
            {
                ModifyFrameMaterial(door, dic["Frame Material"]);
            }
            if (dic.ContainsKey("Frame Color"))
            {
                ModifyFrameColor(door, dic["Frame Color"]);
            }
            if (dic.ContainsKey("Thickness"))
            {
                ModifyThickness(door, double.Parse(dic["Thickness"]));
            }
            if (dic.ContainsKey("Height"))
            {
                ModifyHeight(door, double.Parse(dic["Height"]));
            }
            if (dic.ContainsKey("Trim Projection Exterior"))
            {
                ModifyTrimProjectionExterior(door, double.Parse(dic["Trim Projection Exterior"]));
            }
            if (dic.ContainsKey("Trim Projection Interior"))
            {
                ModifyTrimProjectionInterior(door, double.Parse(dic["Trim Projection Interior"]));
            }
            if (dic.ContainsKey("Trim Width"))
            {
                ModifyTrimWidth(door, double.Parse(dic["Trim Width"]));
            }
            if (dic.ContainsKey("Width"))
            {
                ModifyWidth(door, double.Parse(dic["Width"]));
            }
            if (dic.ContainsKey("Type Comments"))
            {
                ModifyTypeComments(door, dic["Type Comments"]);
            }
        }
    }
}
