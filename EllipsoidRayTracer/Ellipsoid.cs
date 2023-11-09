using System;


namespace rt
{
    public class Ellipsoid : Geometry
    {
        private Vector Center { get; }
        private Vector SemiAxesLength { get; }
        private double Radius { get; }
        
        
        public Ellipsoid(Vector center, Vector semiAxesLength, double radius, Material material, Color color) : base(material, color)
        {
            Center = center;
            SemiAxesLength = semiAxesLength;
            Radius = radius;
        }

        public Ellipsoid(Vector center, Vector semiAxesLength, double radius, Color color) : base(color)
        {
            Center = center;
            SemiAxesLength = semiAxesLength;
            Radius = radius;
        }

        public override Intersection GetIntersection(Line line, double minDist, double maxDist)
        {
            double a = line.Dx.X;
            double c = line.Dx.Y;
            double e = line.Dx.Z;
            
            double b = line.X0.X;
            double d = line.X0.Y;
            double f = line.X0.Z;
            
            double dx = b - Center.X;
            double dy = d - Center.Y;
            double dz = f - Center.Z;
            
            double A = Math.Pow(SemiAxesLength.X, 2);
            double B = Math.Pow(SemiAxesLength.Y, 2);
            double C = Math.Pow(SemiAxesLength.Z, 2);
            
            double m = a * a / A + c * c / B + e * e / C;
            double n = 2 * (a * dx / A + c * dy / B + e * dz / C);
            double p = dx * dx / A + dy * dy / B + dz * dz / C - Radius * Radius;

            var discriminant = n * n - (4 * m * p);
            if (discriminant < 0.001) return new Intersection();
            
            var t1 = (-n - Math.Sqrt(discriminant)) / (2.0 * m);
            var t2 = (-n + Math.Sqrt(discriminant)) / (2.0 * m);

            double t;

            if (t1 > minDist && t2 < maxDist) t = t1;
            else if (t2 > minDist && t2 < maxDist) t = t2;
            else return new Intersection();

            Vector vector = line.CoordinateToPosition(t);
            double vx = vector.X;
            double vy = vector.Y;
            double vz = vector.Z;
            
            return new Intersection(true,true,this,line,t,new Vector(2*vx/A,2*vy/B,2*vz/C).Normalize());
        }
    }
}
