using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;

namespace CreatiomModel
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CreationModel : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            //   var res1=new FilteredElementCollector(doc)
            //       .OfClass(typeof(WallType))
            //       //.Cast<Wall>()
            //       .OfType<WallType>()
            //       .ToList();

            //   var res2 = new FilteredElementCollector(doc)
            //    .OfClass(typeof(FamilyInstance))
            //    .OfCategory(BuiltInCategory.OST_Doors)
            //    .OfType<FamilyInstance>()
            //    .Where(x=>x.Name.Equals("0915 х 2134 мм"))
            //    .ToList();

            //   var res3 = new FilteredElementCollector(doc)
            //.WhereElementIsNotElementType()
            //  .ToList();

            List<Level> listLevel = new FilteredElementCollector(doc)
                   .OfClass(typeof(Level))
                   .OfType<Level>()
                   .ToList();

            Level level1 = listLevel
                .Where(x => x.Name.Equals("Уровень 1"))
                .FirstOrDefault();

            Level level2 = listLevel
               .Where(x => x.Name.Equals("Уровень 2"))
               .FirstOrDefault();

            double width = UnitUtils.ConvertToInternalUnits(10000, UnitTypeId.Millimeters);
            double depth = UnitUtils.ConvertToInternalUnits(5000, UnitTypeId.Millimeters);
            CreateFourWalls(doc, level1, level2, width, depth);

            return Result.Succeeded;
        }


        public void CreateFourWalls(Document doc, Level bottom, Level top, double width, double depth)
        {
            double dx = width / 2;
            double dy = depth / 2;

            List<XYZ> points = new List<XYZ>();
            points.Add(new XYZ(-dx, -dy, 0));
            points.Add(new XYZ(dx, -dy, 0));
            points.Add(new XYZ(dx, dy, 0));
            points.Add(new XYZ(-dx, dy, 0));
            points.Add(new XYZ(-dx, -dy, 0));

            List<Wall> walls = new List<Wall>();

            Transaction transaction = new Transaction(doc, "Create wall");
            transaction.Start();
            for (int i = 0; i < 4; i++)
            {
                Line line = Line.CreateBound(points[i], points[i + 1]);
                Wall wall = Wall.Create(doc, line, bottom.Id, false);
                walls.Add(wall);
                wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(top.Id);
            }
            List<Level> listLevel = new FilteredElementCollector(doc)
                   .OfClass(typeof(Level))
                   .OfType<Level>()
                   .ToList();

            Level level1 = listLevel
                .Where(x => x.Name.Equals("Уровень 1"))
                .FirstOrDefault();
            Level level2 = listLevel
               .Where(x => x.Name.Equals("Уровень 2"))
               .FirstOrDefault();

            AddDoor(doc,level1,walls[0]);
            AddWindow(doc, level1, walls[1]);
            AddWindow(doc, level1, walls[2]);
            AddWindow(doc, level1, walls[3]);
            CreateRoof(doc, level2, walls);

            transaction.Commit();
        }

        //private void AddRoof(Document doc, Level level2, List<Wall> walls)
        //{
        //    RoofType roofType = new FilteredElementCollector(doc)
        //                      .OfClass(typeof(RoofType))
        //                      .OfType<RoofType>()
        //                      .Where(x => x.Name.Equals("Типовой - 400мм"))
        //                      .Where(x => x.FamilyName.Equals("Базовая крыша"))
        //                      .FirstOrDefault();
        //    double wallWidth = walls[0].Width;
        //    double dt = wallWidth / 2;
        //    List<XYZ> points = new List<XYZ>();
        //    points.Add(new XYZ(-dt, -dt, 0));
        //    points.Add(new XYZ(dt, -dt, 0));
        //    points.Add(new XYZ(dt, dt, 0));
        //    points.Add(new XYZ(-dt, dt, 0));
        //    points.Add(new XYZ(-dt, -dt, 0));

        //    Application application = doc.Application;

        //    CurveArray footPrint = application.Create.NewCurveArray();

        //    for (int i = 0; i < 4; i++)
        //    {
        //        LocationCurve curve = walls[i].Location as LocationCurve;
        //        XYZ p1 = curve.Curve.GetEndPoint(0);
        //        XYZ p2 = curve.Curve.GetEndPoint(1);
        //        Line line = Line.CreateBound(p1 + points[i], p2 + points[i+1]);
        //        footPrint.Append(line);
        //    }
        //    ModelCurveArray footPrintToModelCurveMapping = new ModelCurveArray();
        //    FootPrintRoof footprintRoof = doc.Create.NewFootPrintRoof(footPrint, level2, roofType, out footPrintToModelCurveMapping);
        //    //ModelCurveArrayIterator iterator = footPrintToModelCurveMapping.ForwardIterator(); 
        //    //iterator.Reset(); 
        //    //while (iterator.MoveNext()) 
        //    //{ 
        //    //ModelCurve modelCurve = iterator.Current as ModelCurve; 
        //    //footprintRoof.set_DefinesSlope(modelCurve, true); 
        //    //footprintRoof.set_SlopeAngle(modelCurve, 0.5); 
        //    //}

        //    foreach(ModelCurve m in footPrintToModelCurveMapping)
        //    {
        //        footprintRoof.set_DefinesSlope(m, true);
        //        footprintRoof.set_SlopeAngle(m, 0.5);

        //    }
        //}
        public void CreateRoof(Document doc, Level level2, List<Wall> wallList)
        {
            RoofType roofType = new FilteredElementCollector(doc)
                .OfClass(typeof(RoofType))
                .OfType<RoofType>()
                .Where(x => x.Name.Equals("Типовой - 400мм"))
                .Where(x => x.FamilyName.Equals("Базовая крыша"))
                .FirstOrDefault();


            LocationCurve hostCurve = wallList[1].Location as LocationCurve;
            XYZ point3 = hostCurve.Curve.GetEndPoint(0);
            XYZ point4 = hostCurve.Curve.GetEndPoint(1);
            XYZ point = (point3 + point4) / 2;

            double wallWigth = wallList[0].Width;
            double dt = wallWigth / 2;

            XYZ point1 = new XYZ(-dt, -dt, level2.Elevation);
            XYZ point2 = new XYZ(dt, dt, level2.Elevation);

            XYZ A = point3 + point1;
            XYZ B = new XYZ(point.X, point.Y, 20);
            XYZ C = point4 + point2;


            CurveArray curveArray = new CurveArray();
            curveArray.Append(Line.CreateBound(A, B));
            curveArray.Append(Line.CreateBound(B, C));
            using (Transaction tr = new Transaction(doc))
            {
                tr.Start("Create ExtrusionRoof");
                ReferencePlane plane = doc.Create.NewReferencePlane(new XYZ(0, 0, 0), new XYZ(0, 0, 20), new XYZ(0, 20, 0), doc.ActiveView);
                var roof = doc.Create.NewExtrusionRoof(curveArray, plane, level2, roofType, -A.X - wallWigth, A.X + wallWigth);
                roof.get_Parameter(BuiltInParameter.ROOF_EAVE_CUT_PARAM).Set(33619);
                tr.Commit();
            }
        }
            private void AddWindow(Document doc, Level level1, Wall wall)
        {
            FamilySymbol windowType= new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_Windows)
                .OfType<FamilySymbol>()
                .Where(x => x.Name.Equals("0610 x 0610 мм"))
                .Where(x => x.FamilyName.Equals("Фиксированные"))
                .FirstOrDefault();
            LocationCurve hostCurve = wall.Location as LocationCurve;
            XYZ point1 = hostCurve.Curve.GetEndPoint(0);
            XYZ point2 = hostCurve.Curve.GetEndPoint(1);
            XYZ point = (point1 + point2) / 2;

            if (!windowType.IsActive)
            {
                windowType.Activate();
            }

            FamilyInstance familyWindow= doc.Create.NewFamilyInstance(point, windowType, wall, level1, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
            double headHeight = familyWindow.get_Parameter(BuiltInParameter.INSTANCE_HEAD_HEIGHT_PARAM).AsDouble();
            double sillHeight = familyWindow.get_Parameter(BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM).AsDouble();
            double wallHeight = wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).AsDouble();
            familyWindow.get_Parameter(BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM).Set(wallHeight / 2 - (headHeight - sillHeight) / 2);
        }

        private void AddDoor(Document doc, Level level1, Wall wall)
        {
            FamilySymbol doorType = new FilteredElementCollector(doc)
                 .OfClass(typeof(FamilySymbol))
                 .OfCategory(BuiltInCategory.OST_Doors)
                 .OfType<FamilySymbol>()
                 .Where(x => x.Name.Equals("0915 x 2134 мм"))
                 .Where(x => x.FamilyName.Equals("Одиночные-Щитовые"))
                 .FirstOrDefault();

            LocationCurve hostCurve = wall.Location as LocationCurve;
            XYZ point1 = hostCurve.Curve.GetEndPoint(0);
            XYZ point2 = hostCurve.Curve.GetEndPoint(1);
            XYZ point = (point1 + point2) / 2;

            if (!doorType.IsActive)
            {
                doorType.Activate();
            }

            doc.Create.NewFamilyInstance(point, doorType, wall, level1,Autodesk.Revit.DB.Structure.StructuralType.NonStructural );
        }
    }
}
