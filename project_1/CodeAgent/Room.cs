using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CodeAgent
{
    internal class Room
    {
        Document document;
        UV central_point;
        List<UV> points;
        Level level;
        string name;

        // constructor of Room class to create a room in the module
        public Room(Document document, Module module, Level level, string name)
        {
            this.document = document;
            UV point_1 = module.GetSouthwestPoint();
            UV point_2 = module.GetSoutheastPoint();
            UV point_3 = module.GetNortheastPoint();
            UV point_4 = module.GetNorthwestPoint();

            this.central_point = Utils.MidPointForRectangle(point_1, point_2, point_3, point_4);
            this.points = new List<UV>
            {
                point_1,
                point_2,
                point_3,
                point_4
            };
            this.level = level;
            this.name = name;
            Autodesk.Revit.DB.Architecture.Room room = null;
            using (Transaction tr = new Transaction(document, "create_room"))
            {
                tr.Start();
                room = document.Create.NewRoom(level, this.central_point);
                room.Name = name;
                document.Create.NewRoomTag(new LinkElementId(room.Id), this.central_point, document.ActiveView.Id);
                tr.Commit();
            }
        }

        // constructor of Room class to create a room in the module given four boundary points
        public Room(Document document, Module module, UV point_1, UV point_2, UV point_3, UV point_4, Level level, string name)
        {
            this.document = document;

            this.central_point = Utils.MidPointForRectangle(point_1, point_2, point_3, point_4);
            this.points = new List<UV>
            {
                point_1,
                point_2,
                point_3,
                point_4
            };
            this.level = level;
            this.name = name;
            Autodesk.Revit.DB.Architecture.Room room = null;
            using (Transaction tr = new Transaction(document, "create_room"))
            {
                tr.Start();
                room = document.Create.NewRoom(level, this.central_point);
                room.Name = name;
                document.Create.NewRoomTag(new LinkElementId(room.Id), this.central_point, document.ActiveView.Id);
                tr.Commit();
            }
        }

        // constructor of Room class given the located module, the central point, the north-south length, and the east-west length of the room
        public Room(Document document, Module module, UV central_point, double north_south_length, double east_west_length, Level level, string name)
        {
            this.document = document;
            UV point_1 = new UV(central_point.U - east_west_length / 2, central_point.V - north_south_length / 2);
            UV point_2 = new UV(central_point.U + east_west_length / 2, central_point.V - north_south_length / 2);
            UV point_3 = new UV(central_point.U + east_west_length / 2, central_point.V + north_south_length / 2);
            UV point_4 = new UV(central_point.U - east_west_length / 2, central_point.V + north_south_length / 2);
            this.central_point = Utils.MidPointForRectangle(point_1, point_2, point_3, point_4);
            this.points = new List<UV>
            {
                point_1,
                point_2,
                point_3,
                point_4
            };
            this.level = level;
            this.name = name;
            Wall wall_1 = Utils.GetWallByTwoPoints(document, point_1, point_2);
            if (wall_1 == null)
            {
                wall_1 = Utils.CreateWall(this.document, point_1, point_2, this.level, name);
            }
            Wall wall_2 = Utils.GetWallByTwoPoints(document, point_2, point_3);
            if (wall_2 == null)
            {
                wall_2 = Utils.CreateWall(this.document, point_2, point_3, this.level, name);
            }
            Wall wall_3 = Utils.GetWallByTwoPoints(document, point_3, point_4);
            if (wall_3 == null)
            {
                wall_3 = Utils.CreateWall(this.document, point_3, point_4, this.level, name);
            }
            Wall wall_4 = Utils.GetWallByTwoPoints(document, point_4, point_1);
            if (wall_4 == null)
            {
                wall_4 = Utils.CreateWall(this.document, point_4, point_1, this.level, name);
            }
            Autodesk.Revit.DB.Architecture.Room room = null;
            using (Transaction tr = new Transaction(document, "create_room"))
            {
                tr.Start();
                room = document.Create.NewRoom(level, this.central_point);
                room.Name = name;
                document.Create.NewRoomTag(new LinkElementId(room.Id), this.central_point, document.ActiveView.Id);
                tr.Commit();
            }
        }

        // constructor of Room class given the located module, the direction of the room in the module, and the length of the room
        public Room(Document document, Module module, string direction, double length, Level level, string name)
        {
            this.document = document;
            UV point_1;
            UV point_2;
            UV point_3;
            UV point_4;
            if (direction == "south")
            {
                point_1 = module.GetSouthwestPoint();
                point_2 = module.GetSoutheastPoint();
                point_3 = new UV(point_2.U, point_2.V + length);
                point_4 = new UV(point_1.U, point_1.V + length);
            }
            else if (direction == "north")
            {
                point_1 = module.GetNorthwestPoint();
                point_2 = module.GetNortheastPoint();
                point_3 = new UV(point_2.U, point_2.V - length);
                point_4 = new UV(point_1.U, point_1.V - length);
            }
            else if (direction == "east")
            {
                point_1 = module.GetSoutheastPoint();
                point_2 = module.GetNortheastPoint();
                point_3 = new UV(point_2.U - length, point_2.V);
                point_4 = new UV(point_1.U - length, point_1.V);
            }
            else // direction == "west"
            {
                point_1 = module.GetSouthwestPoint();
                point_2 = module.GetNorthwestPoint();
                point_3 = new UV(point_2.U + length, point_2.V);
                point_4 = new UV(point_1.U + length, point_1.V);
            }
            this.central_point = Utils.MidPointForRectangle(point_1, point_2, point_3, point_4);
            this.points = new List<UV>
            {
                point_1,
                point_2,
                point_3,
                point_4
            };
            this.level = level;
            this.name = name;
            Wall wall_1 = Utils.GetWallByTwoPoints(document, point_1, point_2);
            if (wall_1 == null)
            {
                wall_1 = Utils.CreateWall(this.document, point_1, point_2, this.level, name);
            }
            Wall wall_2 = Utils.GetWallByTwoPoints(document, point_2, point_3);
            if (wall_2 == null)
            {
                wall_2 = Utils.CreateWall(this.document, point_2, point_3, this.level, name);
            }
            Wall wall_3 = Utils.GetWallByTwoPoints(document, point_3, point_4);
            if (wall_3 == null)
            {
                wall_3 = Utils.CreateWall(this.document, point_3, point_4, this.level, name);
            }
            Wall wall_4 = Utils.GetWallByTwoPoints(document, point_4, point_1);
            if (wall_4 == null)
            {
                wall_4 = Utils.CreateWall(this.document, point_4, point_1, this.level, name);
            }
            Autodesk.Revit.DB.Architecture.Room room = null;
            using (Transaction tr = new Transaction(document, "create_room"))
            {
                tr.Start();
                room = document.Create.NewRoom(level, this.central_point);
                room.Name = name;
                document.Create.NewRoomTag(new LinkElementId(room.Id), this.central_point, document.ActiveView.Id);
                tr.Commit();
            }
        }

        // constructor of Room class given the located module, the corner orientation of the room in the module, and the length of the room from the north-south and east-west direction
        public Room(Document document, Module module, string corner, double north_south_length, double east_west_length, Level level, string name)
        {
            this.document = document;
            UV point_1;
            UV point_2;
            UV point_3;
            UV point_4;
            if (corner == "southeast")
            {
                point_1 = module.GetSoutheastPoint();
                point_2 = new UV(point_1.U, point_1.V + north_south_length);
                point_3 = new UV(point_1.U - east_west_length, point_1.V + north_south_length);
                point_4 = new UV(point_1.U - east_west_length, point_1.V);
            }
            else if (corner == "southwest")
            {
                point_1 = module.GetSouthwestPoint();
                point_2 = new UV(point_1.U, point_1.V + north_south_length);
                point_3 = new UV(point_1.U + east_west_length, point_1.V + north_south_length);
                point_4 = new UV(point_1.U + east_west_length, point_1.V);
            }
            else if (corner == "northwest")
            {
                point_1 = module.GetNorthwestPoint();
                point_2 = new UV(point_1.U, point_1.V - north_south_length);
                point_3 = new UV(point_1.U + east_west_length, point_1.V - north_south_length);
                point_4 = new UV(point_1.U + east_west_length, point_1.V);
            }
            else // corner == "northeast"
            {
                point_1 = module.GetNortheastPoint();
                point_2 = new UV(point_1.U, point_1.V - north_south_length);
                point_3 = new UV(point_1.U - east_west_length, point_1.V - north_south_length);
                point_4 = new UV(point_1.U - east_west_length, point_1.V);
            }
            this.central_point = Utils.MidPointForRectangle(point_1, point_2, point_3, point_4);
            this.points = new List<UV>
            {
                point_1,
                point_2,
                point_3,
                point_4
            };
            this.level = level;
            this.name = name;
            Wall wall_1 = Utils.GetWallByTwoPoints(document, point_1, point_2);
            if (wall_1 == null)
            {
                wall_1 = Utils.CreateWall(this.document, point_1, point_2, this.level, name);
            }
            Wall wall_2 = Utils.GetWallByTwoPoints(document, point_2, point_3);
            if (wall_2 == null)
            {
                wall_2 = Utils.CreateWall(this.document, point_2, point_3, this.level, name);
            }
            Wall wall_3 = Utils.GetWallByTwoPoints(document, point_3, point_4);
            if (wall_3 == null)
            {
                wall_3 = Utils.CreateWall(this.document, point_3, point_4, this.level, name);
            }
            Wall wall_4 = Utils.GetWallByTwoPoints(document, point_4, point_1);
            if (wall_4 == null)
            {
                wall_4 = Utils.CreateWall(this.document, point_4, point_1, this.level, name);
            }
            Autodesk.Revit.DB.Architecture.Room room = null;
            using (Transaction tr = new Transaction(document, "create_room"))
            {
                tr.Start();
                UV central_point = Utils.MidPointForRectangle(point_1, point_2, point_3, point_4);
                room = document.Create.NewRoom(level, this.central_point);
                room.Name = name;
                document.Create.NewRoomTag(new LinkElementId(room.Id), this.central_point, document.ActiveView.Id);
                tr.Commit();
            }
        }

        // get the southeast point of the room
        public UV GetSoutheastPoint()
        {
            List<double> Us = new List<double>();
            foreach (UV point in this.points)
            {
                Us.Add(point.U);
            }
            Us.Sort();
            List<double> Vs = new List<double>();
            foreach (UV point in this.points)
            {
                Vs.Add(point.V);
            }
            Vs.Sort();
            return new UV(Us[Us.Count() - 1], Vs[0]);
        }

        // get the southwest point of the room
        public UV GetSouthwestPoint()
        {
            List<double> Us = new List<double>();
            foreach (UV point in this.points)
            {
                Us.Add(point.U);
            }
            Us.Sort();
            List<double> Vs = new List<double>();
            foreach (UV point in this.points)
            {
                Vs.Add(point.V);
            }
            Vs.Sort();
            return new UV(Us[0], Vs[0]);
        }

        // get the northwest point of the room
        public UV GetNorthwestPoint()
        {
            List<double> Us = new List<double>();
            foreach (UV point in this.points)
            {
                Us.Add(point.U);
            }
            Us.Sort();
            List<double> Vs = new List<double>();
            foreach (UV point in this.points)
            {
                Vs.Add(point.V);
            }
            Vs.Sort();
            return new UV(Us[0], Vs[Vs.Count() - 1]);
        }

        // get the northeast point of the room
        public UV GetNortheastPoint()
        {
            List<double> Us = new List<double>();
            foreach (UV point in this.points)
            {
                Us.Add(point.U);
            }
            Us.Sort();
            List<double> Vs = new List<double>();
            foreach (UV point in this.points)
            {
                Vs.Add(point.V);
            }
            Vs.Sort();
            return new UV(Us[Us.Count() - 1], Vs[Vs.Count() - 1]);
        }

        // get the east wall of a room
        public Wall GetEastWall()
        {
            UV point_1 = GetNortheastPoint();
            UV point_2 = GetSoutheastPoint();
            return Utils.GetWallByTwoPoints(this.document, point_1, point_2);
        }

        // get the south wall of a room
        public Wall GetSouthWall()
        {
            UV point_1 = GetSoutheastPoint();
            UV point_2 = GetSouthwestPoint();
            return Utils.GetWallByTwoPoints(this.document, point_1, point_2);
        }

        // get the west wall of a room
        public Wall GetWestWall()
        {
            UV point_1 = GetSouthwestPoint();
            UV point_2 = GetNorthwestPoint();
            return Utils.GetWallByTwoPoints(this.document, point_1, point_2);
        }

        // get the north wall of a room
        public Wall GetNorthWall()
        {
            UV point_1 = GetNortheastPoint();
            UV point_2 = GetNorthwestPoint();
            return Utils.GetWallByTwoPoints(this.document, point_1, point_2);
        }
    }
}
