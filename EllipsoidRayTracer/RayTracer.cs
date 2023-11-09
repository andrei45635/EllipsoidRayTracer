using System;
using System.Runtime.InteropServices;

namespace rt
{
    class RayTracer
    {
        private Geometry[] geometries;
        private Light[] lights;

        public RayTracer(Geometry[] geometries, Light[] lights)
        {
            this.geometries = geometries;
            this.lights = lights;
        }

        private double ImageToViewPlane(int n, int imgSize, double viewPlaneSize)
        {
            return -n * viewPlaneSize / imgSize + viewPlaneSize / 2;
        }

        private Intersection FindFirstIntersection(Line ray, double minDist, double maxDist)
        {
            var intersection = new Intersection();

            foreach (var geometry in geometries)
            {
                var intr = geometry.GetIntersection(ray, minDist, maxDist);

                if (!intr.Valid || !intr.Visible) continue;

                if (!intersection.Valid || !intersection.Visible)
                {
                    intersection = intr;
                }
                else if (intr.T < intersection.T)
                {
                    intersection = intr;
                }
            }

            return intersection;
        }

        private bool IsLit(Vector point, Light light)
        {
            // TODO: ADD CODE HERE
            Line line = new Line(light.Position, point);
            Intersection intersection = FindFirstIntersection(line, 0, point.Length() - 0.0001);
            if (!intersection.Valid || !intersection.Visible)
            {
                return true;
            }
            return intersection.T > (light.Position - point).Length() - 0.001;
        }

        public double Relu(double x)
        {
            if (x < 0) return 0;
            return x;
        }
        
        public void Render(Camera camera, int width, int height, string filename)
        {
            var background = new Color();

            var image = new Image(width, height);

            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    // TODO: ADD CODE HERE
                    var pointOnView = camera.Position + camera.Direction * camera.ViewPlaneDistance +
                                      (camera.Up ^ camera.Direction) *
                                      ImageToViewPlane(i, width, camera.ViewPlaneWidth) +
                                      camera.Up * ImageToViewPlane(j, height, camera.ViewPlaneHeight);
                    var ray = new Line(camera.Position, pointOnView);
                    var intersection = FindFirstIntersection(ray, camera.FrontPlaneDistance, camera.BackPlaneDistance);
                    if (intersection.Valid && intersection.Visible)
                    {
                        Color color = new Color();
                        foreach (var light in lights)
                        {
                            Color lightColor = new Color();
                            lightColor += intersection.Geometry.Material.Ambient * light.Ambient;
                            var E = (camera.Position - intersection.Position).Normalize(); // v = intersection.Position
                            var T = (light.Position - intersection.Position).Normalize();
                            //var N = ((Sphere)intersection.Geometry).Normal(intersection.Position);
                            var N = intersection.Normal;
                            var R = (N * (N * T) * 2 - T).Normalize();
                            if (IsLit(intersection.Position, light))
                            {
                                if (N * T > 0)
                                {
                                    lightColor += intersection.Geometry.Material.Diffuse * light.Diffuse * (N * T);
                                }
                                
                                if (E * R > 0)
                                {
                                    lightColor += intersection.Geometry.Material.Specular * light.Specular *
                                                  Math.Pow(E * R, intersection.Geometry.Material.Shininess);
                                }

                                lightColor *= light.Intensity;
                            }
                            color += lightColor;
                        }
                        image.SetPixel(i, j, color);
                    } 
                    else image.SetPixel(i, j, background);
                }
            }
            image.Store(filename);
        }
    }
}