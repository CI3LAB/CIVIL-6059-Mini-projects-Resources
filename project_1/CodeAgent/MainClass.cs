using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAgent
{
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class MainClass : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Level level = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .WhereElementIsNotElementType()
                .Cast<Level>()
                .FirstOrDefault(x => x.Name == "Level 0");

            // example_1
            // 2-dimensional boundary points of module_1
            /*UV point_1 = new UV(0, 0);
            UV point_2 = new UV(10, 0);
            UV point_3 = new UV(10, 20);
            UV point_4 = new UV(0, 20);
            // 2-dimensional boundary points of module_2
            UV point_5 = new UV(10, 0);
            UV point_6 = new UV(20, 0);
            UV point_7 = new UV(20, 20);
            UV point_8 = new UV(10, 20);
            // create modules
            Module module_1 = new Module(doc, point_1, point_2, point_3, point_4, level, "module_1");
            Module module_2 = new Module(doc, point_5, point_6, point_7, point_8, level, "module_2");
            // create rooms in the modules
            Room bedroom = new Room(doc, module_1, level, "Bedroom");
            Room living_room = new Room(doc, module_2, level, "Living room");
            // create a door between the bedroom and the living room
            FamilyInstance Door_1 = Utils.CreateDoorBetweenRooms(doc, bedroom, living_room, level);
            // create a door between the living room and the external space
            FamilyInstance Door_2 = Utils.CreateDoorOrWindowBetweenRoomAndExternalSpace(doc, living_room, level, "south", "door");
            // create a window between the living room and the external space
            FamilyInstance Window_1 = Utils.CreateDoorOrWindowBetweenRoomAndExternalSpace(doc, living_room, level, "north", "window");
            // create a window between the bedroom and the external space
            FamilyInstance Window_2 = Utils.CreateDoorOrWindowBetweenRoomAndExternalSpace(doc, bedroom, level, "north", "window");*/

            // example_2
            // 2-dimensional boundary points of module_1
            UV point_1 = new UV(0, 0);
            UV point_2 = new UV(20, 0);
            UV point_3 = new UV(20, 10);
            UV point_4 = new UV(0, 10);
            // 2-dimensional boundary points of module_2
            UV point_5 = new UV(0, 10);
            UV point_6 = new UV(20, 10);
            UV point_7 = new UV(20, 20);
            UV point_8 = new UV(0, 20);
            // create modules
            Module module_1 = new Module(doc, point_1, point_2, point_3, point_4, level, "module_1");
            Module module_2 = new Module(doc, point_5, point_6, point_7, point_8, level, "module_2");
            // merge module_1 and module_2
            Module new_module = Utils.MergeModule(doc, module_1, module_2, level);
            // calculate dividing line for Bedroom and Living Room
            UV dividingPoint1 = new UV(6.67, 0);
            UV dividingPoint2 = new UV(6.67, 20);
            // create dividing wall
            Wall dividingWall = Utils.CreateWall(doc, dividingPoint1, dividingPoint2, level, "DividingWall");
            // create Bedroom and Living Room
            UV bedroomCenter = Utils.MidPointForRectangle(point_1, dividingPoint1, dividingPoint2, point_8);
            UV livingRoomCenter = Utils.MidPointForRectangle(dividingPoint1, point_2, point_7, dividingPoint2);
            Room bedroom = new Room(doc, new_module, point_1, dividingPoint1, dividingPoint2, point_8, level, "Bedroom");
            Room living_room = new Room(doc, new_module, dividingPoint1, point_2, point_7, dividingPoint2, level, "Living room");
            // install door between Bedroom and Living Room
            Wall sharedWall = Utils.FindSharedWall(doc, bedroom, living_room, level);
            UV doorPoint = Utils.MidPointForWall(sharedWall);
            FamilyInstance Door = Utils.CreateDoor(doc, doorPoint, sharedWall, level);

            return Result.Succeeded;
        }

    }
}
