using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIM
{
    internal class StairFunc
    {
        // modification
        public static void Modify(Stairs stair, string property_name, string property_value)
        {
            switch (property_name)
            {
                case "Base Level":
                    ModifyBaseLevel(stair, property_value);
                    break;
                case "Base Offset":
                    ModifyBaseOffset(stair, double.Parse(property_value));
                    break;
                case "Top Level":
                    ModifyTopLevel(stair, property_value);
                    break;
                case "Top Offset":
                    ModifyTopOffset(stair, double.Parse(property_value));
                    break;
                case "Desired Stair Height":
                    ModifyDesiredStairHeight(stair, double.Parse(property_value));
                    break;
                case "Desired Number Of Risers":
                    ModifyDesiredNumberOfRisers(stair, int.Parse(property_value));
                    break;
                case "Actual Tread Depth":
                    ModifyActualTreadDepth(stair, double.Parse(property_value));
                    break;
                case "Comments":
                    ModifyComments(stair, property_value);
                    break;
                case "Maximum Riser Height":
                    ModifyMaximumRiserHeight(stair, double.Parse(property_value));
                    break;
                case "Minimum Tread Depth":
                    ModifyMinimumTreadDepth(stair, double.Parse(property_value));
                    break;
                case "Minimum Run Width":
                    ModifyMinimumRunWidth(stair, double.Parse(property_value));
                    break;
                case "Function":
                    ModifyFunction(stair, int.Parse(property_value));
                    break;
                case "Right Support":
                    ModifyRightSupport(stair, int.Parse(property_value));
                    break;
                case "Right Lateral Offset":
                    ModifyRightLateralOffset(stair, double.Parse(property_value));
                    break;
                case "Left Support":
                    ModifyLeftSupport(stair, int.Parse(property_value));
                    break;
                case "Left Lateral Offset":
                    ModifyLeftLateralOffset(stair, double.Parse(property_value));
                    break;
                case "Middle Support":
                    ModifyMiddleSupport(stair, int.Parse(property_value));
                    break;
                case "Middle Support Number":
                    ModifyMiddleSupportNumber(stair, int.Parse(property_value));
                    break;
                case "Type Comments":
                    ModifyTypeComments(stair, property_value);
                    break;
            }
        }

        // instance properties
        public static void ModifyBaseLevel(Stairs stair, string level_str)
        {
            // find the level id with the given name
            Level level = new FilteredElementCollector(stair.Document)
                .OfClass(typeof(Level))
                .Where(x => x.Name == level_str)
                .Cast<Level>()
                .FirstOrDefault();
            // modify the base level of the stair
            using (Transaction tr = new Transaction(stair.Document, "modify_stair_base_level"))
            {
                tr.Start();
                stair.get_Parameter(BuiltInParameter.STAIRS_BASE_LEVEL_PARAM).Set(level.Id);
                tr.Commit();
            }
        }

        public static void ModifyBaseOffset(Stairs stair, double offset)
        {
            // modify the base offset of the stair
            // the metric of offest here is feet
            using (Transaction tr = new Transaction(stair.Document, "modify_stair_base_offset"))
            {
                tr.Start();
                stair.get_Parameter(BuiltInParameter.STAIRS_BASE_OFFSET).Set(offset);
                tr.Commit();
            }
        }

        public static void ModifyTopLevel(Stairs stair, string level_str)
        {
            // find the level id with the given name
            Level level = new FilteredElementCollector(stair.Document)
                .OfClass(typeof(Level))
                .Where(x => x.Name == level_str)
                .Cast<Level>()
                .FirstOrDefault();
            // modify the top level of the stair
            using (Transaction tr = new Transaction(stair.Document, "modify_stair_top_level"))
            {
                tr.Start();
                if (level_str == "None")
                    stair.get_Parameter(BuiltInParameter.STAIRS_TOP_LEVEL_PARAM).Set(ElementId.InvalidElementId);
                else
                    stair.get_Parameter(BuiltInParameter.STAIRS_TOP_LEVEL_PARAM).Set(level.Id);
                tr.Commit();
            }
        }

        public static void ModifyTopOffset(Stairs stair, double offset)
        {
            // modify the top offset of the stair
            // the metric of offest here is feet
            using (Transaction tr = new Transaction(stair.Document, "modify_stair_top_offset"))
            {
                tr.Start();
                stair.get_Parameter(BuiltInParameter.STAIRS_TOP_OFFSET).Set(offset);
                tr.Commit();
            }
        }

        public static void ModifyDesiredStairHeight(Stairs stair, double height)
        {
            // modify the desired stair height of the stair
            // the metric of height here is feet
            using (Transaction tr = new Transaction(stair.Document, "modify_stair_desired_height"))
            {
                tr.Start();
                stair.get_Parameter(BuiltInParameter.STAIRS_STAIRS_HEIGHT).Set(height);
                tr.Commit();
            }
        }

        public static void ModifyDesiredNumberOfRisers(Stairs stair, int number)
        {
            // modify the desired number of risers of the stair
            using (Transaction tr = new Transaction(stair.Document, "modify_stair_desired_number_of_risers"))
            {
                tr.Start();
                stair.get_Parameter(BuiltInParameter.STAIRS_DESIRED_NUMBER_OF_RISERS).Set(number);
                tr.Commit();
            }
        }

        public static void ModifyActualTreadDepth(Stairs stair, double depth)
        {
            // modify the actual tread depth of the stair
            // the metric of depth here is feet
            using (Transaction tr = new Transaction(stair.Document, "modify_stair_actual_tread_depth"))
            {
                tr.Start();
                stair.get_Parameter(BuiltInParameter.STAIRS_ACTUAL_TREAD_DEPTH).Set(depth);
                tr.Commit();
            }
        }

        public static void ModifyComments(Stairs stair, string comment)
        {
            // modify the comments of the stair
            using (Transaction tr = new Transaction(stair.Document, "modify_stair_comments"))
            {
                tr.Start();
                stair.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set(comment);
                tr.Commit();
            }
        }

        // type properties
        public static void ModifyMaximumRiserHeight(Stairs stair, double height)
        {
            // modify the maximum riser height of the stair
            // the metric of here is feet
            // duplicate the type
            StairsType stairsType = stair.Document.GetElement(stair.GetTypeId()) as StairsType;
            string origin_name = stairsType.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(stair.Document, "modify_stair_maximum_riser_height"))
            {
                tr.Start();
                ElementId new_type_id = stairsType.Duplicate(origin_name + "_maximum_riser_height_" + height.ToString()).Id;
                StairsType newStairType = stair.Document.GetElement(new_type_id) as StairsType;
                newStairType.get_Parameter(BuiltInParameter.STAIRS_ATTR_MAX_RISER_HEIGHT).Set(height);
                stair.ChangeTypeId(new_type_id);
                tr.Commit();
            }
        }

        public static void ModifyMinimumTreadDepth(Stairs stair, double depth)
        {
            // modify the minimum tread depth of the stair
            // the metric of here is feet
            // duplicate the type
            StairsType stairsType = stair.Document.GetElement(stair.GetTypeId()) as StairsType;
            string origin_name = stairsType.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(stair.Document, "modify_stair_minimum_tread_depth"))
            {
                tr.Start();
                ElementId new_type_id = stairsType.Duplicate(origin_name + "_minimum_tread_depth_" + depth.ToString()).Id;
                StairsType newStairType = stair.Document.GetElement(new_type_id) as StairsType;
                newStairType.get_Parameter(BuiltInParameter.STAIRS_ATTR_MINIMUM_TREAD_DEPTH).Set(depth);
                stair.ChangeTypeId(new_type_id);
                tr.Commit();
            }
        }

        public static void ModifyMinimumRunWidth(Stairs stair, double width)
        {
            // modify the minimum run width of the stair
            // the metric of here is feet
            // duplicate the type
            StairsType stairsType = stair.Document.GetElement(stair.GetTypeId()) as StairsType;
            string origin_name = stairsType.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(stair.Document, "modify_stair_minimum_run_width"))
            {
                tr.Start();
                ElementId new_type_id = stairsType.Duplicate(origin_name + "_minimum_run_width_" + width.ToString()).Id;
                StairsType newStairType = stair.Document.GetElement(new_type_id) as StairsType;
                newStairType.get_Parameter(BuiltInParameter.STAIRSTYPE_MINIMUM_RUN_WIDTH).Set(width);
                stair.ChangeTypeId(new_type_id);
                tr.Commit();
            }
        }

        public static void ModifyFunction(Stairs stair, int fun)
        {
            // modify the function of the stair type
            // 0->Interior, 1->Exterior
            // duplicate the type
            StairsType stairsType = stair.Document.GetElement(stair.GetTypeId()) as StairsType;
            string origin_name = stairsType.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(stair.Document, "modify_stair_function"))
            {
                tr.Start();
                ElementId new_type_id = stairsType.Duplicate(origin_name + "_function_" + fun.ToString()).Id;
                StairsType newStairType = stair.Document.GetElement(new_type_id) as StairsType;
                newStairType.get_Parameter(BuiltInParameter.FUNCTION_PARAM).Set(fun);
                stair.ChangeTypeId(new_type_id);
                tr.Commit();
            }
        }

        public static void ModifyRightSupport(Stairs stair, int option)
        {
            // modify the right support of the stair type
            // 0->None, 1->Stringer (Closed), 2->Carriage (Open)
            // duplicate the type
            StairsType stairsType = stair.Document.GetElement(stair.GetTypeId()) as StairsType;
            string origin_name = stairsType.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(stair.Document, "modify_stair_right_support"))
            {
                tr.Start();
                ElementId new_type_id = stairsType.Duplicate(origin_name + "_right_support_" + option.ToString()).Id;
                StairsType newStairType = stair.Document.GetElement(new_type_id) as StairsType;
                newStairType.get_Parameter(BuiltInParameter.STAIRSTYPE_HAS_RIGHT_SUPPORT).Set(option);
                stair.ChangeTypeId(new_type_id);
                tr.Commit();
            }
        }

        public static void ModifyRightLateralOffset(Stairs stair, double offest)
        {
            // modify the right lateral offset of the stair type
            // the metric of offest here is feet
            // duplicate the type
            StairsType stairsType = stair.Document.GetElement(stair.GetTypeId()) as StairsType;
            string origin_name = stairsType.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(stair.Document, "modify_stair_right_lateral_offset"))
            {
                tr.Start();
                ElementId new_type_id = stairsType.Duplicate(origin_name + "_right_lateral_offset_" + offest.ToString()).Id;
                StairsType newStairType = stair.Document.GetElement(new_type_id) as StairsType;
                newStairType.get_Parameter(BuiltInParameter.STAIRSTYPE_RIGHT_SUPPORT_LATERAL_OFFSET).Set(offest);
                stair.ChangeTypeId(new_type_id);
                tr.Commit();
            }
        }

        public static void ModifyLeftSupport(Stairs stair, int option)
        {
            // modify the left support of the stair type
            // 0->None, 1->Stringer (Closed), 2->Carriage (Open)
            // duplicate the type
            StairsType stairsType = stair.Document.GetElement(stair.GetTypeId()) as StairsType;
            string origin_name = stairsType.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(stair.Document, "modify_stair_left_support"))
            {
                tr.Start();
                ElementId new_type_id = stairsType.Duplicate(origin_name + "_left_support_" + option.ToString()).Id;
                StairsType newStairType = stair.Document.GetElement(new_type_id) as StairsType;
                newStairType.get_Parameter(BuiltInParameter.STAIRSTYPE_HAS_LEFT_SUPPORT).Set(option);
                stair.ChangeTypeId(new_type_id);
                tr.Commit();
            }
        }

        public static void ModifyLeftLateralOffset(Stairs stair, double offest)
        {
            // modify the left lateral offset of the stair type
            // the metric of offest here is feet
            // duplicate the type
            StairsType stairsType = stair.Document.GetElement(stair.GetTypeId()) as StairsType;
            string origin_name = stairsType.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(stair.Document, "modify_stair_left_lateral_offset"))
            {
                tr.Start();
                ElementId new_type_id = stairsType.Duplicate(origin_name + "_left_lateral_offset_" + offest.ToString()).Id;
                StairsType newStairType = stair.Document.GetElement(new_type_id) as StairsType;
                newStairType.get_Parameter(BuiltInParameter.STAIRSTYPE_LEFT_SUPPORT_LATERAL_OFFSET).Set(offest);
                stair.ChangeTypeId(new_type_id);
                tr.Commit();
            }
        }

        public static void ModifyMiddleSupport(Stairs stair, int option)
        {
            // modify the middle support of the stair type
            // 0->No, 1->Yes
            // duplicate the type
            StairsType stairsType = stair.Document.GetElement(stair.GetTypeId()) as StairsType;
            string origin_name = stairsType.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(stair.Document, "modify_middle_support"))
            {
                tr.Start();
                ElementId new_type_id = stairsType.Duplicate(origin_name + "_middle_support_" + option.ToString()).Id;
                StairsType newStairType = stair.Document.GetElement(new_type_id) as StairsType;
                newStairType.get_Parameter(BuiltInParameter.STAIRSTYPE_HAS_INTERMEDIATE_SUPPORT).Set(option);
                stair.ChangeTypeId(new_type_id);
                tr.Commit();
            }
        }

        public static void ModifyMiddleSupportNumber(Stairs stair, int number)
        {
            // modify the middle support number of the stair type
            // duplicate the type
            StairsType stairsType = stair.Document.GetElement(stair.GetTypeId()) as StairsType;
            string origin_name = stairsType.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(stair.Document, "modify_middle_support_number"))
            {
                tr.Start();
                ElementId new_type_id = stairsType.Duplicate(origin_name + "_middle_support_number_" + number.ToString()).Id;
                StairsType newStairType = stair.Document.GetElement(new_type_id) as StairsType;
                newStairType.get_Parameter(BuiltInParameter.STAIRSTYPE_NUMBER_OF_INTERMEDIATE_SUPPORTS).Set(number);
                stair.ChangeTypeId(new_type_id);
                tr.Commit();
            }
        }

        public static void ModifyTypeComments(Stairs stair, string comment)
        {
            // modify the comments of the stair type
            // duplicate the type
            StairsType stairsType = stair.Document.GetElement(stair.GetTypeId()) as StairsType;
            string origin_name = stairsType.Name;
            // modify the property of the new type
            using (Transaction tr = new Transaction(stair.Document, "modify_stair_type_comments"))
            {
                tr.Start();
                ElementId new_type_id = stairsType.Duplicate(origin_name + "_type_comments_" + comment).Id;
                StairsType newStairType = stair.Document.GetElement(new_type_id) as StairsType;
                newStairType.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS).Set(comment);
                stair.ChangeTypeId(new_type_id);
                tr.Commit();
            }
        }

        // retrieval
        public static void Retrieval(Stairs stair, string option)
        {
            // retrieve corresponding properties of the stair and display in the Revit UI
            switch (option)
            {
                case "Base Level":
                    TaskDialog.Show("Base Level", stair.get_Parameter(BuiltInParameter.STAIRS_BASE_LEVEL_PARAM).AsValueString());
                    break;
                case "Base Offset":
                    TaskDialog.Show("Base Offset", stair.get_Parameter(BuiltInParameter.STAIRS_BASE_OFFSET).AsValueString());
                    break;
                case "Top Level":
                    TaskDialog.Show("Top Level", stair.get_Parameter(BuiltInParameter.STAIRS_TOP_LEVEL_PARAM).AsValueString());
                    break;
                case "Top Offset":
                    TaskDialog.Show("Top Offset", stair.get_Parameter(BuiltInParameter.STAIRS_TOP_OFFSET).AsValueString());
                    break;
                case "Desired Stair Height":
                    TaskDialog.Show("Desired Stair Height", stair.get_Parameter(BuiltInParameter.STAIRS_STAIRS_HEIGHT).AsValueString());
                    break;
                case "Desired Number Of Risers":
                    TaskDialog.Show("Desired Number Of Risers", stair.get_Parameter(BuiltInParameter.STAIRS_DESIRED_NUMBER_OF_RISERS).AsValueString());
                    break;
                case "Actual Tread Depth":
                    TaskDialog.Show("Actual Tread Depth", stair.get_Parameter(BuiltInParameter.STAIRS_ACTUAL_TREAD_DEPTH).AsValueString());
                    break;
                case "Comments":
                    TaskDialog.Show("Comments", stair.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString());
                    break;
                case "Maximum Riser Height":
                    TaskDialog.Show("Maximum Riser Height", stair.Document.GetElement(stair.GetTypeId()).get_Parameter(BuiltInParameter.STAIRS_ATTR_MAX_RISER_HEIGHT).AsValueString());
                    break;
                case "Minimum Tread Depth":
                    TaskDialog.Show("Minimum Tread Depth", stair.Document.GetElement(stair.GetTypeId()).get_Parameter(BuiltInParameter.STAIRS_ATTR_MINIMUM_TREAD_DEPTH).AsValueString());
                    break;
                case "Minimum Run Width":
                    TaskDialog.Show("Minimum Run Width", stair.Document.GetElement(stair.GetTypeId()).get_Parameter(BuiltInParameter.STAIRSTYPE_MINIMUM_RUN_WIDTH).AsValueString());
                    break;
                case "Function":
                    TaskDialog.Show("Function", stair.Document.GetElement(stair.GetTypeId()).get_Parameter(BuiltInParameter.FUNCTION_PARAM).AsValueString());
                    break;
                case "Right Support":
                    TaskDialog.Show("Right Support", stair.Document.GetElement(stair.GetTypeId()).get_Parameter(BuiltInParameter.STAIRSTYPE_HAS_RIGHT_SUPPORT).AsValueString());
                    break;
                case "Right Lateral Offset":
                    TaskDialog.Show("Right Lateral Offset", stair.Document.GetElement(stair.GetTypeId()).get_Parameter(BuiltInParameter.STAIRSTYPE_RIGHT_SUPPORT_LATERAL_OFFSET).AsValueString());
                    break;
                case "Left Support":
                    TaskDialog.Show("Left Support", stair.Document.GetElement(stair.GetTypeId()).get_Parameter(BuiltInParameter.STAIRSTYPE_HAS_LEFT_SUPPORT).AsValueString());
                    break;
                case "Left Lateral Offset":
                    TaskDialog.Show("Left Lateral Offset", stair.Document.GetElement(stair.GetTypeId()).get_Parameter(BuiltInParameter.STAIRSTYPE_LEFT_SUPPORT_LATERAL_OFFSET).AsValueString());
                    break;
                case "Middle Support":
                    TaskDialog.Show("Middle Support", stair.Document.GetElement(stair.GetTypeId()).get_Parameter(BuiltInParameter.STAIRSTYPE_HAS_INTERMEDIATE_SUPPORT).AsValueString());
                    break;
                case "Middle Support Number":
                    TaskDialog.Show("Middle Support Number", stair.Document.GetElement(stair.GetTypeId()).get_Parameter(BuiltInParameter.STAIRSTYPE_NUMBER_OF_INTERMEDIATE_SUPPORTS).AsValueString());
                    break;
                case "Type Comments":
                    TaskDialog.Show("Type Comments", stair.Document.GetElement(stair.GetTypeId()).get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS).AsString());
                    break;
                case "Actual Number Of Risers":
                    TaskDialog.Show("Actual Number Of Risers", stair.get_Parameter(BuiltInParameter.STAIRS_ACTUAL_NUM_RISERS).AsValueString());
                    break;
                case "Actual Riser Height":
                    TaskDialog.Show("Actual Riser Height", stair.get_Parameter(BuiltInParameter.STAIRS_ACTUAL_RISER_HEIGHT).AsValueString());
                    break;
                case "Coordinate":
                    BoundingBoxXYZ bounding = stair.get_BoundingBox(stair.Document.ActiveView);
                    TaskDialog.Show("Coordinate", "Max: " + bounding.Max.ToString() + "\nMin: " + bounding.Min.ToString());
                    break;
            }
        }

        // deletion
        public static void Delete(Stairs stair)
        {
            using (Transaction tr = new Transaction(stair.Document, "delete_stair"))
            {
                tr.Start();
                stair.Document.Delete(stair.Id);
                tr.Commit();
            }
        }

        // creation
        public static void Create(Document doc, XYZ p1, XYZ p2, Dictionary<string, string> dic)
        {
            Level level = new FilteredElementCollector(doc)
               .OfClass(typeof(Level))
               .First() as Level;
            if (dic.ContainsKey("Base Level"))
            {
                string base_level = dic["Base Level"];
                level = new FilteredElementCollector(doc)
                    .OfClass(typeof(Level))
                    .Where(x => x.Name == base_level)
                    .Cast<Level>()
                    .FirstOrDefault();
            }

            StairsType stairsType = new FilteredElementCollector(doc)
                .OfClass(typeof(StairsType))
                .WhereElementIsElementType()
                .Cast<StairsType>()
                .FirstOrDefault(x => x.FamilyName == "Assembled Stair");

            Level top_level = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .Where(x => (x as Level).Elevation > level.Elevation)
                .Cast<Level>()
                .FirstOrDefault();
            if (dic.ContainsKey("Top Level"))
            {
                string top_level_str = dic["Top Level"];
                level = new FilteredElementCollector(doc)
                    .OfClass(typeof(Level))
                    .Where(x => x.Name == top_level_str)
                    .Cast<Level>()
                    .FirstOrDefault();
            }

            // create a straight stairs using these two points
            ElementId newStairsId = null;

            using (StairsEditScope newStairsScope = new StairsEditScope(doc, "create_stairs"))
            {
                newStairsId = newStairsScope.Start(level.Id, top_level.Id);

                using (Transaction stairsTrans = new Transaction(doc, "add_runs_to_stairs"))
                {
                    stairsTrans.Start();
                    StairsRun straightRun = StairsRun.CreateStraightRun(doc, newStairsId, Line.CreateBound(p1, p2), StairsRunJustification.Center);
                    stairsTrans.Commit();

                }
                // define error handling
                newStairsScope.Commit(new Utils.FailuresPreprocessor());
            }
            // remove the railings
            List<ElementId> railing_ids = (doc.GetElement(newStairsId) as Stairs).GetAssociatedRailings().ToList();
            using (Transaction tr = new Transaction(doc, "delete_railings"))
            {
                tr.Start();
                foreach (ElementId id in railing_ids)
                {
                    doc.Delete(id);
                }
                tr.Commit();
            }

            // modify based on the user arguments
            Stairs stair = doc.GetElement(newStairsId) as Stairs;
            if (dic.ContainsKey("Base Offset"))
            {
                ModifyBaseOffset(stair, double.Parse(dic["Base Offset"]));
            }
            if (dic.ContainsKey("Top Offset"))
            {
                ModifyTopOffset(stair, double.Parse(dic["Top Offset"]));
            }
            if (dic.ContainsKey("Desired Stair Height"))
            {
                ModifyDesiredStairHeight(stair, double.Parse(dic["Desired Stair Height"]));
            }
            if (dic.ContainsKey("Desired Number Of Risers"))
            {
                ModifyDesiredNumberOfRisers(stair, int.Parse(dic["Desired Number Of Risers"]));
            }
            if (dic.ContainsKey("Actual Tread Depth"))
            {
                ModifyActualTreadDepth(stair, double.Parse(dic["Actual Tread Depth"]));
            }
            if (dic.ContainsKey("Comments"))
            {
                ModifyComments(stair, dic["Comments"]);
            }
            if (dic.ContainsKey("Maximum Riser Height"))
            {
                ModifyMaximumRiserHeight(stair, double.Parse(dic["Maximum Riser Height"]));
            }
            if (dic.ContainsKey("Minimum Tread Depth"))
            {
                ModifyMinimumTreadDepth(stair, double.Parse(dic["Minimum Tread Depth"]));
            }
            if (dic.ContainsKey("Minimum Run Width"))
            {
                ModifyMinimumRunWidth(stair, double.Parse(dic["Minimum Run Width"]));
            }
            if (dic.ContainsKey("Function"))
            {
                ModifyFunction(stair, int.Parse(dic["Function"]));
            }
            if (dic.ContainsKey("Right Support"))
            {
                ModifyRightSupport(stair, int.Parse(dic["Right Support"]));
            }
            if (dic.ContainsKey("Right Lateral Offset"))
            {
                ModifyRightLateralOffset(stair, double.Parse(dic["Right Lateral Offset"]));
            }
            if (dic.ContainsKey("Left Support"))
            {
                ModifyLeftSupport(stair, int.Parse(dic["Left Support"]));
            }
            if (dic.ContainsKey("Left Lateral Offset"))
            {
                ModifyLeftLateralOffset(stair, double.Parse(dic["Left Lateral Offset"]));
            }
            if (dic.ContainsKey("Middle Support"))
            {
                ModifyMiddleSupport(stair, int.Parse(dic["Middle Support"]));
            }
            if (dic.ContainsKey("Middle Support Number"))
            {
                ModifyMiddleSupportNumber(stair, int.Parse(dic["Middle Support Number"]));
            }
            if (dic.ContainsKey("Type Comments"))
            {
                ModifyTypeComments(stair, dic["Type Comments"]);
            }
        }
    }
}

