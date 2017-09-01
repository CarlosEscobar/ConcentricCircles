using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.Geometry;
using System;

[assembly: CommandClass(typeof(AutoCadAddIns.ConcentricCircles.ConcentricCircles))]

namespace AutoCadAddIns.ConcentricCircles
{
    /// <summary>
    /// Class defining commands for the concentric circles add in.
    /// </summary>
    public static class ConcentricCircles
    {
        /// <summary>
        /// Correlative for BlockName.
        /// </summary>
        public static int BlockCorrelative = 1;
        /// <summary>
        /// Indicates the number of circles in the concentric circle.
        /// </summary>
        public static int numberOfCircles = 1;
        /// <summary>
        /// Factor to expand/contract the concentric circle.
        /// </summary>
        public static double radiusFactor = 1.0;
        /// <summary>
        /// X coordinate for the concentric circle center.
        /// </summary>
        public static double centerX = 0.0;
        /// <summary>
        /// Y coordinate for the concentric circle center.
        /// </summary>
        public static double centerY = 0.0;
        /// <summary>
        /// Array with RGB values for each circle.
        /// </summary>
        public static string[] circlesColors = { "0,0,0", "0,0,0", "0,0,0", "0,0,0", "0,0,0",
                                                 "0,0,0", "0,0,0", "0,0,0", "0,0,0", "0,0,0",
                                                 "0,0,0", "0,0,0", "0,0,0", "0,0,0", "0,0,0",
                                                 "0,0,0", "0,0,0", "0,0,0", "0,0,0", "0,0,0"};
        
        /// <summary>
        /// Command to open the concentric circle form
        /// </summary>
        [CommandMethod("ConcentricCircles")]
        public static void DoConcentricCircles()
        {
            //Show the Concentric Circles Form.
            ConcentricCirclesForm concentricCirclesForm = new ConcentricCirclesForm();
            Application.ShowModelessDialog(concentricCirclesForm);
        }

        /// <summary>
        /// Command to draw the concentric circle
        /// </summary>
        [CommandMethod("DrawConcentricCircles")]
        public static void DrawConcentricCircles()
        {
            //Get current autocad application document and database
            Document currentDocument = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database currentDatabase = currentDocument.Database;
            //Open a database transaction
            using (Transaction acadTransaction = currentDatabase.TransactionManager.StartTransaction())
            {
                //Using try catch to rollback and abort on error
                try
                {
                    //Database table BlockTable opened in read mode
                    BlockTable acadBlockTable;
                    acadBlockTable = acadTransaction.GetObject(currentDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                    //Database table BlockTable opened in write mode
                    BlockTableRecord acadBlockTableRecordWrite;
                    acadBlockTableRecordWrite = acadTransaction.GetObject(acadBlockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                    
                    //Define block name
                    string concentricCircleBlockName = "ConcentricCircle" + BlockCorrelative++;
                    //Create Concentric Circle block
                    BlockTableRecord concentricCircleBlockRecord = new BlockTableRecord();
                    concentricCircleBlockRecord.Name = concentricCircleBlockName;
                    //Upgrades the block table object to write mode
                    acadBlockTable.UpgradeOpen();
                    //Add Concentric Circle block to database
                    ObjectId concentricCircleBlockId = acadBlockTable.Add(concentricCircleBlockRecord);
                    acadTransaction.AddNewlyCreatedDBObject(concentricCircleBlockRecord, true);

                    //Center of concentric circle
                    Point3d center = new Point3d(centerX, centerY, 0.0);
                    //Normal vector to circle
                    Vector3d normal = new Vector3d(0, 0, 1.0);
                    //Temporal circle variabe
                    Circle acadCircle;
                    //Temporal array to hold RGB values for circle
                    string[] currentCircleColors;
                    //Variables to hold object ids.
                    ObjectIdCollection idFirstCircle = new ObjectIdCollection();
                    ObjectIdCollection idLastCircle = new ObjectIdCollection();

                    //Add circles
                    for (int i = 1; i < (numberOfCircles + 1); i++)
                    {
                        using (acadCircle = new Circle(center, normal, i * radiusFactor))
                        {
                            //Get RGB values for circle
                            currentCircleColors = circlesColors[i - 1].Split(',');
                            acadCircle.Color = Color.FromRgb(Convert.ToByte(currentCircleColors[0]), Convert.ToByte(currentCircleColors[1]), Convert.ToByte(currentCircleColors[2]));
                            //Define circle thickness
                            acadCircle.Thickness = 3.0;
                            //Add circle to database
                            ObjectId circleId = concentricCircleBlockRecord.AppendEntity(acadCircle);
                            acadTransaction.AddNewlyCreatedDBObject(acadCircle, true);
                            //Store first and last circle ids for hatching boundaries
                            if (i == 1) { idFirstCircle.Add(acadCircle.ObjectId); }
                            if (i == numberOfCircles) { idLastCircle.Add(acadCircle.ObjectId); }
                        }
                    }
                    
                    //Add hatching
                    using (Hatch acadHatch = new Hatch())
                    {
                        //Add hatch to database
                        concentricCircleBlockRecord.AppendEntity(acadHatch);
                        acadTransaction.AddNewlyCreatedDBObject(acadHatch, true);
                        //Set hatch pattern
                        acadHatch.SetHatchPattern(HatchPatternType.PreDefined, "ANSI31");
                        acadHatch.Associative = true;
                        //Set hatch boundaries (first and last circle)
                        acadHatch.AppendLoop(HatchLoopTypes.Outermost, idFirstCircle);
                        acadHatch.AppendLoop(HatchLoopTypes.Default, idLastCircle);
                        acadHatch.EvaluateHatch(true);
                    }
                    
                    //Define Concentric Circle Block Reference
                    BlockReference concentricCircleBlockReference = new BlockReference(Point3d.Origin, concentricCircleBlockId);
                    //Add Concentric Circle Block Reference to database
                    acadBlockTableRecordWrite.AppendEntity(concentricCircleBlockReference);
                    acadTransaction.AddNewlyCreatedDBObject(concentricCircleBlockReference, true);

                    //Commit database changes.
                    acadTransaction.Commit();
                }
                catch
                {
                    //Rollback and abort database changes on error.
                    acadTransaction.Abort();
                }
            }
        }

    }
}