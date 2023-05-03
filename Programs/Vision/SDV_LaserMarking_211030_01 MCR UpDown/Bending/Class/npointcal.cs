using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cognex.VisionPro.CalibFix;
using Cognex.VisionPro;
using Cognex.VisionPro.Display;
using Cognex.VisionPro.Dimensioning;

namespace Bending
{
    public class npointcal
    {
        
        ICogTransform2D trans2D;
        public void NpointCal(ICogImage cogimg, List<ucRecipe.MapPosition> pos, double rawsize)
        {
            CogCalibCheckerboardTool t = new CogCalibCheckerboardTool();
            string d = t.OutputImage.SelectedSpaceName;
            CogCalibNPointToNPointTool NPointTool = new CogCalibNPointToNPointTool();
            NPointTool.InputImage = cogimg;

            NPointTool.Calibration.AddPointPair(pos[0].x, pos[0].y, 0, 0);
            NPointTool.Calibration.AddPointPair(pos[1].x, pos[1].y, 2, 0);
            NPointTool.Calibration.AddPointPair(pos[2].x, pos[2].y, 4, 0);
            NPointTool.Calibration.AddPointPair(pos[3].x, pos[3].y, 0, 2);
            NPointTool.Calibration.AddPointPair(pos[4].x, pos[4].y, 0, 4);

            NPointTool.Calibration.Calibrate();
            NPointTool.Run();

            string calspace = NPointTool.OutputImage.SelectedSpaceName;
            trans2D = NPointTool.OutputImage.GetTransform(calspace, "#");
        }

        public ucRecipe.MapPosition transPoint(ucRecipe.MapPosition pos)
        {
            ucRecipe.MapPosition result = new ucRecipe.MapPosition(0,0);
            
            double inputx = pos.x;
            double inputy = pos.y;
            double outx, outy;
            trans2D.MapPoint(inputx, inputy, out outx, out outy);
            Console.WriteLine("out X = " + outx.ToString());
            Console.WriteLine("out Y = " + outy.ToString());
            result.x = outx;
            result.y = outy;
            return result;
        }

        public void distance(ICogImage cogimg, ucRecipe.MapPosition _pos1, ucRecipe.MapPosition _pos2, ref double dist, ref double angle)
        {
            ucRecipe.MapPosition pos1 = transPoint(_pos1);
            ucRecipe.MapPosition pos2 = transPoint(_pos2);
            CogDistancePointPointTool pp = new CogDistancePointPointTool();
            pp.InputImage = cogimg;
            pp.StartX = pos1.x;
            pp.StartY = pos1.y;
            pp.EndX = pos2.x;
            pp.EndY = pos2.y;
            pp.Run();

            dist = pp.Distance;
            angle = pp.Angle;
        }
    }
}
