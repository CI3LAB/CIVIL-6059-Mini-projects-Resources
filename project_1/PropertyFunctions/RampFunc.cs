using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace BIM
{
    internal class RampFunc
    {
        // modification
        public static void Modify(Element ramp, string property_name, string property_value)
        {
            switch (property_name)
            {
                case "Base Level":
                    ModifyBaseLevel(ramp, property_value);
                    break;
                case "Base Offset":
                    ModifyBaseOffset(ramp, Convert.ToDouble(property_value));
                    break;
                case "Top Level":
                    ModifyTopLevel(ramp, property_value);
                    break;
                case "Top Offset":
                    ModifyTopOffset(ramp, Convert.ToDouble(property_value));
                    break;
                case "Multistory Top Level":
                    ModifyMultistoryTopLevel(ramp, property_value);
                    break;
                case "Width":
                    ModifWidth(ramp, Convert.ToDouble(property_value));
                    break;
                case "Comments":
                    ModifyComments(ramp, property_value);
                    break;
                case "Shape":
                    ModifyShape(ramp, Convert.ToInt32(property_value));
                    break;
                case "Thickness":
                    ModifyThickness(ramp, Convert.ToDouble(property_value));
                    break;
                case "Function":
                    ModifyFunction(ramp, Convert.ToInt32(property_value));
                    break;
                case "Material":
                    ModifyMaterial(ramp, property_value);
                    break;
                case "Color":
                    ModifyColor(ramp, property_value);
                    break;
                case "Maximum Incline Length":
                    ModifyMaximumInclineLength(ramp, Convert.ToDouble(property_value));
                    break;
                case "Maximum Slope":
                    ModifyMaxSlope(ramp, Convert.ToDouble(property_value));
                    break;
                case "Type Comments":
                    ModifyTypeComments(ramp, property_value);
                    break;
            }
        }

        // instance properties
        public static void ModifyBaseLevel(Element ramp, string level_str)
        {
            // find the level id with the given name
            Level level = new FilteredElementCollector(ramp.Document)
                .OfClass(typeof(Level))
                .Where(x => x.Name == level_str)
                .Cast<Level>()
                .FirstOrDefault();
            // modify the base level
            using (Transaction tr = new Transaction(ramp.Document, "modify_ramp_base_level"))
            {
                tr.Start();
                ramp.get_Parameter(BuiltInParameter.STAIRS_BASE_LEVEL_PARAM).Set(level.Id);
                tr.Commit();
            }
        }

        public static void ModifyBaseOffset(Element ramp, double offset)
        {
            // modify the base offset of the ramp
            // the metric of offest here is feet
            using (Transaction tr = new Transaction(ramp.Document, "modify_ramp_base_offset"))
            {
                tr.Start();
                ramp.get_Parameter(BuiltInParameter.STAIRS_BASE_OFFSET).Set(offset);
                tr.Commit();
            }
        }

        public static void ModifyTopLevel(Element ramp, string level_str)
        {
            // find the level id with the given name
            Level level = new FilteredElementCollector(ramp.Document)
                .OfClass(typeof(Level))
                .Where(x => x.Name == level_str)
                .Cast<Level>()
                .FirstOrDefault();
            // modify the top level of the ramp
            using (Transaction tr = new Transaction(ramp.Document, "modify_ramp_top_level"))
            {
                tr.Start();
                if (level_str == "None")
                    ramp.get_Parameter(BuiltInParameter.STAIRS_TOP_LEVEL_PARAM).Set(ElementId.InvalidElementId);
                else
                    ramp.get_Parameter(BuiltInParameter.STAIRS_TOP_LEVEL_PARAM).Set(level.Id);
                tr.Commit();
            }
        }

        public static void ModifyTopOffset(Element ramp, double offset)
        {
            // modify the top offset of the ramp
            // the metric of offest here is feet
            using (Transaction tr = new Transaction(ramp.Document, "modify_ramp_top_offset"))
            {
                tr.Start();
                ramp.get_Parameter(BuiltInParameter.STAIRS_TOP_OFFSET).Set(offset);
                tr.Commit();
            }
        }

        public static void ModifyMultistoryTopLevel(Element ramp, string level_str)
        {
            // find the level id with the given name
            Level level = new FilteredElementCollector(ramp.Document)
                .OfClass(typeof(Level))
                .Where(x => x.Name == level_str)
                .Cast<Level>()
                .FirstOrDefault();
            // modify the multistory top level of the ramp
            using (Transaction tr = new Transaction(ramp.Document, "modify_ramp_multistory_top_level"))
            {
                tr.Start();
                if (level_str == "None")
                    ramp.get_Parameter(BuiltInParameter.STAIRS_MULTISTORY_TOP_LEVEL_PARAM).Set(ElementId.InvalidElementId);
                else
                    ramp.get_Parameter(BuiltInParameter.STAIRS_MULTISTORY_TOP_LEVEL_PARAM).Set(level.Id);
                tr.Commit();
            }
        }

        public static void ModifWidth(Element ramp, double width)
        {
            // modify the width of the ramp
            // the metric of width here is feet
            using (Transaction tr = new Transaction(ramp.Document, "modify_ramp_width"))
            {
                tr.Start();
                ramp.get_Parameter(BuiltInParameter.STAIRS_ATTR_TREAD_WIDTH).Set(width);
                tr.Commit();
            }
        }

        public static void ModifyComments(Element ramp, string comment)
        {
            // modify the comments of the ramp
            using (Transaction tr = new Transaction(ramp.Document, "modify_ramp_comments"))
            {
                tr.Start();
                ramp.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set(comment);
                tr.Commit();
            }
        }

        // type properties
        public static void ModifyShape(Element ramp, int option)
        {
            // modify the function of the ramp type
            // 0->Thick, 1->Solid
            // duplicate the type
            ElementType rampType = ramp.Document.GetElement(ramp.GetTypeId()) as ElementType;
            string origin_name = rampType.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(ramp.Document, "modify_ramp_shape"))
            {
                tr.Start();
                ElementId new_type_id = rampType.Duplicate(origin_name + "_shape_" + option.ToString()).Id;
                ElementType newRampType = ramp.Document.GetElement(new_type_id) as ElementType;
                newRampType.get_Parameter(BuiltInParameter.RAMP_ATTR_SHAPE).Set(option);
                ramp.ChangeTypeId(new_type_id);
                tr.Commit();
            }
        }

        public static void ModifyThickness(Element ramp, double thickness)
        {
            // modify the minimum thickness of the ramp
            // the metric of here is feet
            // duplicate the type
            ElementType elementType = ramp.Document.GetElement(ramp.GetTypeId()) as ElementType;
            string origin_name = elementType.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(ramp.Document, "modify_ramp_thickness"))
            {
                tr.Start();
                ElementId new_type_id = elementType.Duplicate(origin_name + "_thickness_" + thickness.ToString()).Id;
                ElementType newRampType = ramp.Document.GetElement(new_type_id) as ElementType;
                newRampType.get_Parameter(BuiltInParameter.RAMP_ATTR_THICKNESS).Set(thickness);
                ramp.ChangeTypeId(new_type_id);
                tr.Commit();
            }
        }

        public static void ModifyFunction(Element ramp, int fun)
        {
            // modify the function of the ramp type
            // 0->Interior, 1->Exterior
            // duplicate the type
            ElementType rampType = ramp.Document.GetElement(ramp.GetTypeId()) as ElementType;
            string origin_name = rampType.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(ramp.Document, "modify_ramp_function"))
            {
                tr.Start();
                ElementId new_type_id = rampType.Duplicate(origin_name + "_function_" + fun.ToString()).Id;
                ElementType newRampType = ramp.Document.GetElement(new_type_id) as ElementType;
                newRampType.get_Parameter(BuiltInParameter.FUNCTION_PARAM).Set(fun);
                ramp.ChangeTypeId(new_type_id);
                tr.Commit();
            }
        }

        public static void ModifyMaterial(Element ramp, string material_name)
        {
            // modify the material of the ramp type
            // duplicate the ramp type of the ramp
            ElementType elementType = ramp.Document.GetElement(ramp.GetTypeId()) as ElementType;
            string origin_name = elementType.Name;
            // get all the mateirals
            FilteredElementCollector collector = new FilteredElementCollector(ramp.Document);
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
            using (Transaction tr = new Transaction(ramp.Document, "modify_ramp_material"))
            {
                tr.Start();
                ElementId new_type_id = elementType.Duplicate(origin_name + "_material_" + material_name).Id;
                ElementType newRampType = ramp.Document.GetElement(new_type_id) as ElementType;
                newRampType.get_Parameter(BuiltInParameter.RAMP_ATTR_MATERIAL).Set(materialIds[min_index]);
                ramp.ChangeTypeId(new_type_id);
                tr.Commit();
            }
        }

        public static void ModifyColor(Element ramp, string color)
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
            // duplicate the ramp type of the ramp
            ElementType elementType = ramp.Document.GetElement(ramp.GetTypeId()) as ElementType;
            string origin_name = elementType.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(ramp.Document, "modify_ramp_color"))
            {
                tr.Start();
                ElementId new_type_id = elementType.Duplicate(origin_name + "_color_" + color).Id;
                ElementType newRampType = ramp.Document.GetElement(new_type_id) as ElementType;
                Material material = ramp.Document.GetElement(newRampType.get_Parameter(BuiltInParameter.RAMP_ATTR_MATERIAL).AsElementId()) as Material;
                material = material.Duplicate(material.Name + "_color_" + color);
                material.Color = color_map[color];
                newRampType.get_Parameter(BuiltInParameter.RAMP_ATTR_MATERIAL).Set(material.Id);
                ramp.ChangeTypeId(new_type_id);
                tr.Commit();
            }
        }

        public static void ModifyMaximumInclineLength(Element ramp, double length)
        {
            // modify the maximum incline length of the ramp
            // the metric of length here is feet
            // duplicate the ramp type of the ramp
            ElementType elementType = ramp.Document.GetElement(ramp.GetTypeId()) as ElementType;
            string origin_name = elementType.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(ramp.Document, "modify_ramp_maximum_incline_length"))
            {
                tr.Start();
                ElementId new_type_id = elementType.Duplicate(origin_name + "_maximum_incline_length_" + length.ToString()).Id;
                ElementType newRampType = ramp.Document.GetElement(new_type_id) as ElementType;
                newRampType.get_Parameter(BuiltInParameter.RAMP_MAX_RUN_LENGTH).Set(length);
                ramp.ChangeTypeId(new_type_id);
                tr.Commit();
            }
        }

        public static void ModifyMaxSlope(Element ramp, double slope)
        {
            // modify the maximum slope of the ramp
            // duplicate the ramp type of the ramp
            ElementType elementType = ramp.Document.GetElement(ramp.GetTypeId()) as ElementType;
            string origin_name = elementType.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(ramp.Document, "modify_ramp_maximum_slope"))
            {
                tr.Start();
                ElementId new_type_id = elementType.Duplicate(origin_name + "_maximum_slope_" + slope.ToString()).Id;
                ElementType newRampType = ramp.Document.GetElement(new_type_id) as ElementType;
                newRampType.get_Parameter(BuiltInParameter.RAMP_ATTR_MIN_INV_SLOPE).Set(slope);
                ramp.ChangeTypeId(new_type_id);
                tr.Commit();
            }
        }

        public static void ModifyTypeComments(Element ramp, string comment)
        {
            // modify the comments of the ramp type
            ElementType elementType = ramp.Document.GetElement(ramp.GetTypeId()) as ElementType;
            string origin_name = elementType.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(ramp.Document, "modify_ramp_type_comments"))
            {
                tr.Start();
                ElementId new_type_id = elementType.Duplicate(origin_name + "_comments_" + comment).Id;
                ElementType newRampType = ramp.Document.GetElement(new_type_id) as ElementType;
                newRampType.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS).Set(comment);
                ramp.ChangeTypeId(new_type_id);
                tr.Commit();
            }
        }

        // retrieval
        public static void Retrieval(Element ramp, string option)
        {
            // retrieve corresponding properties of the ramp and display in the Revit UI
            switch (option)
            {
                case "Base Level":
                    TaskDialog.Show("Base Level", ramp.get_Parameter(BuiltInParameter.STAIRS_BASE_LEVEL_PARAM).AsValueString());
                    break;
                case "Base Offset":
                    TaskDialog.Show("Base Offset", ramp.get_Parameter(BuiltInParameter.STAIRS_BASE_OFFSET).AsValueString());
                    break;
                case "Top Level":
                    TaskDialog.Show("Top Level", ramp.get_Parameter(BuiltInParameter.STAIRS_TOP_LEVEL_PARAM).AsValueString());
                    break;
                case "Top Offset":
                    TaskDialog.Show("Top Offset", ramp.get_Parameter(BuiltInParameter.STAIRS_TOP_OFFSET).AsValueString());
                    break;
                case "Multistory Top Level":
                    TaskDialog.Show("Multistory Top Level", ramp.get_Parameter(BuiltInParameter.STAIRS_MULTISTORY_TOP_LEVEL_PARAM).AsValueString());
                    break;
                case "Width":
                    TaskDialog.Show("Width", ramp.get_Parameter(BuiltInParameter.STAIRS_ATTR_TREAD_WIDTH).AsValueString());
                    break;
                case "Comments":
                    TaskDialog.Show("Comments", ramp.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString());
                    break;
                case "Shape":
                    ElementType rampType = ramp.Document.GetElement(ramp.GetTypeId()) as ElementType;
                    TaskDialog.Show("Shape", rampType.get_Parameter(BuiltInParameter.RAMP_ATTR_SHAPE).AsValueString());
                    break;
                case "Thickness":
                    ElementType elementType = ramp.Document.GetElement(ramp.GetTypeId()) as ElementType;
                    TaskDialog.Show("Thickness", elementType.get_Parameter(BuiltInParameter.RAMP_ATTR_THICKNESS).AsValueString());
                    break;
                case "Function":
                    ElementType elementType1 = ramp.Document.GetElement(ramp.GetTypeId()) as ElementType;
                    TaskDialog.Show("Function", elementType1.get_Parameter(BuiltInParameter.FUNCTION_PARAM).AsValueString());
                    break;
                case "Material":
                    ElementType elementType2 = ramp.Document.GetElement(ramp.GetTypeId()) as ElementType;
                    Material material = ramp.Document.GetElement(elementType2.get_Parameter(BuiltInParameter.RAMP_ATTR_MATERIAL).AsElementId()) as Material;
                    TaskDialog.Show("Material", material.Name);
                    break;
                case "Color":
                    ElementType elementType3 = ramp.Document.GetElement(ramp.GetTypeId()) as ElementType;
                    Material material1 = ramp.Document.GetElement(elementType3.get_Parameter(BuiltInParameter.RAMP_ATTR_MATERIAL).AsElementId()) as Material;
                    Color color = material1.Color;
                    TaskDialog.Show("Color (RGB)", color.Red.ToString() + " " + color.Green.ToString() + " " + color.Blue.ToString());
                    break;
                case "Maximum Incline Length":
                    ElementType elementType4 = ramp.Document.GetElement(ramp.GetTypeId()) as ElementType;
                    TaskDialog.Show("Maximum Incline Length", elementType4.get_Parameter(BuiltInParameter.RAMP_MAX_RUN_LENGTH).AsValueString());
                    break;
                case "Maximum Slope":
                    ElementType elementType5 = ramp.Document.GetElement(ramp.GetTypeId()) as ElementType;
                    TaskDialog.Show("Maximum Slope", elementType5.get_Parameter(BuiltInParameter.RAMP_ATTR_MIN_INV_SLOPE).AsValueString());
                    break;
                case "Type Comments":
                    ElementType elementType6 = ramp.Document.GetElement(ramp.GetTypeId()) as ElementType;
                    TaskDialog.Show("Type Comments", elementType6.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS).AsString());
                    break;
                case "Coordinate":
                    BoundingBoxXYZ bounding = ramp.get_BoundingBox(ramp.Document.ActiveView);
                    TaskDialog.Show("Coordinate", "Max: " + bounding.Max.ToString() + "\nMin: " + bounding.Min.ToString());
                    break;
            }
        }

        // deletion
        public static void Delete(Element ramp)
        {
            using (Transaction tr = new Transaction(ramp.Document, "delete_ramp"))
            {
                tr.Start();
                ramp.Document.Delete(ramp.Id);
                tr.Commit();
            }
        }

        // creation
        public static void Create(Document doc, XYZ p1, XYZ p2, Dictionary<string, string> dic)
        {
            /*
             * Since the API regarding creating a ramp is unavailable in Revit API
             * We leave this part of work as future work
             * Unlike general class (e.g., wall) or familyinstance (e.g., door)
             * The API of ramp is not exposed in the Revit API document
             * */
        }
    }
}
