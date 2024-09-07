using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAgent
{
    internal class Module
    {
        Document document;
        List<UV> points;
        Level level;
        string name;

        // constructor of Module class given four boundary points
        public Module(Document document, UV point_1, UV point_2, UV point_3, UV point_4, Level level, string name)
        {
            this.document = document;
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
            Floor floor = Utils.CreateFloor(document, point_1, point_2, point_3, point_4, level, name);
        }

        // constructor of Module class given two modules
        public Module(Document document, Module module_1, Module module_2)
        {
            this.document = document;
            this.points = module_1.points.Union(module_2.points).ToList();
            this.name = module_1.name + " " + module_2.name;
        }

        // get the southeast point of the module
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
            return new UV(Us[Us.Count()-1], Vs[0]);
        }

        // get the southwest point of the module
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

        // get the northwest point of the module
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

        // get the northeast point of the module
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

        // get the east wall of the module
        public Wall GetEastWall()
        {
            UV point_1 = GetNortheastPoint();
            UV point_2 = GetSoutheastPoint();
            return Utils.GetWallByTwoPoints(this.document, point_1, point_2);
        }

        // get the south wall of the module
        public Wall GetSouthWall()
        {
            UV point_1 = GetSoutheastPoint();
            UV point_2 = GetSouthwestPoint();
            return Utils.GetWallByTwoPoints(this.document, point_1, point_2);
        }

        // get the west wall of the module
        public Wall GetWestWall()
        {
            UV point_1 = GetSouthwestPoint();
            UV point_2 = GetNorthwestPoint();
            return Utils.GetWallByTwoPoints(this.document, point_1, point_2);
        }

        // get the north wall of the module
        public Wall GetNorthWall()
        {
            UV point_1 = GetNortheastPoint();
            UV point_2 = GetNorthwestPoint();
            return Utils.GetWallByTwoPoints(this.document, point_1, point_2);
        }

        // create a new module to the south of the current module
        // north_south_length is the length of the module from the north-south direction, east_west_length is the length of the module from the east-west direction
        // alignment has two options 'west' and 'east', indicating whether the new module is aligned west or east of the current module
        public Module CreateModuleToSouth(double north_south_length, double east_west_length, string alignment)
        {
            UV point_west = GetSouthwestPoint();
            UV point_east = GetSoutheastPoint();
            UV point_1;
            UV point_2;
            UV point_3;
            UV point_4;
            if (alignment == "west")
            {
                point_1 = new UV(point_west.U, point_west.V);
                point_2 = new UV(point_west.U + east_west_length, point_west.V);
                point_3 = new UV(point_west.U, point_west.V - north_south_length);
                point_4 = new UV(point_west.U + east_west_length, point_west.V - north_south_length);
            }
            else // alignment == "east"
            {
                point_1 = new UV(point_east.U, point_east.V);
                point_2 = new UV(point_east.U - east_west_length, point_east.V);
                point_3 = new UV(point_east.U, point_east.V - north_south_length);
                point_4 = new UV(point_east.U - east_west_length, point_east.V - north_south_length);
            }
            return new Module(this.document, point_1, point_2, point_3, point_4, this.level, this.name);
        }

        // create a new module to the north of the current module
        // north_south_length is the length of the module from the north-south direction, east_west_length is the length of the module from the east-west direction
        // alignment has two options 'west' and 'east', indicating whether the new module is aligned west or east of the current module
        public Module CreateModuleToNorth(double north_south_length, double east_west_length, string alignment)
        {
            UV point_west = GetNorthwestPoint();
            UV point_east = GetNortheastPoint();
            UV point_1;
            UV point_2;
            UV point_3;
            UV point_4;
            if (alignment == "west")
            {
                point_1 = new UV(point_west.U, point_west.V);
                point_2 = new UV(point_west.U + east_west_length, point_west.V);
                point_3 = new UV(point_west.U, point_west.V + north_south_length);
                point_4 = new UV(point_west.U + east_west_length, point_west.V + north_south_length);
            }
            else // alignment == "east"
            {
                point_1 = new UV(point_east.U, point_east.V);
                point_2 = new UV(point_east.U - east_west_length, point_east.V);
                point_3 = new UV(point_east.U, point_east.V + north_south_length);
                point_4 = new UV(point_east.U - east_west_length, point_east.V + north_south_length);
            }
            return new Module(this.document, point_1, point_2, point_3, point_4, this.level, this.name);
        }

        // create a new module to the east of the current module
        // north_south_length is the length of the module from the north-south direction, east_west_length is the length of the module from the east-west direction
        // alignment has two options 'north' and 'south', indicating whether the new module is aligned north or south of the current module
        public Module CreateModuleToEast(double north_south_length, double east_west_length, string alignment)
        {
            UV point_north = GetNortheastPoint();
            UV point_south = GetSoutheastPoint();
            UV point_1;
            UV point_2;
            UV point_3;
            UV point_4;
            if (alignment == "north")
            {
                point_1 = new UV(point_north.U, point_north.V);
                point_2 = new UV(point_north.U + east_west_length, point_north.V);
                point_3 = new UV(point_north.U, point_north.V - north_south_length);
                point_4 = new UV(point_north.U + east_west_length, point_north.V - north_south_length);
            }
            else // alignment == "south"
            {
                point_1 = new UV(point_south.U, point_south.V);
                point_2 = new UV(point_south.U + east_west_length, point_south.V);
                point_3 = new UV(point_south.U, point_south.V + north_south_length);
                point_4 = new UV(point_south.U + east_west_length, point_south.V + north_south_length);
            }
            return new Module(this.document, point_1, point_2, point_3, point_4, this.level, this.name);
        }

        // create a new module to the west of the current module
        // north_south_length is the length of the module from the north-south direction, east_west_length is the length of the module from the east-west direction
        // alignment has two options 'north' and 'south', indicating whether the new module is aligned north or south of the current module
        public Module CreateModuleToWest(double north_south_length, double east_west_length, string alignment)
        {
            UV point_north = GetNortheastPoint();
            UV point_south = GetSoutheastPoint();
            UV point_1;
            UV point_2;
            UV point_3;
            UV point_4;
            if (alignment == "north")
            {
                point_1 = new UV(point_north.U, point_north.V);
                point_2 = new UV(point_north.U - east_west_length, point_north.V);
                point_3 = new UV(point_north.U, point_north.V - north_south_length);
                point_4 = new UV(point_north.U - east_west_length, point_north.V - north_south_length);
            }
            else // alignment == "south"
            {
                point_1 = new UV(point_south.U, point_south.V);
                point_2 = new UV(point_south.U - east_west_length, point_south.V);
                point_3 = new UV(point_south.U, point_south.V + north_south_length);
                point_4 = new UV(point_south.U - east_west_length, point_south.V + north_south_length);
            }
            return new Module(this.document, point_1, point_2, point_3, point_4, this.level, this.name);
        }
    }
}
