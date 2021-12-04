using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.MapProviders;
using System.Net;
using System.Web.Script.Serialization;

namespace Algo_project_2
{
    public partial class Form1 : Form
    {
        List<Location> locationsListBank = new List<Location>();
        bool showHull = false; //for toggling 
        List<Location> locationsList = new List<Location>();
        List<GMapMarker> markersList = new List<GMapMarker>();
       
        Location pivotLocation;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            GetLocationData();


            gmap.MapProvider = GoogleMapProvider.Instance;
            GMaps.Instance.Mode = AccessMode.ServerOnly;
      
            gmap.Position = new PointLatLng(-46.4133000, 168.3553000); //set SIT as the centre
            gmap.ShowCenter = false;

            GMapOverlay markers = new GMapOverlay("markers");
            // put th pin on each location
            foreach (Location l in locationsList)
            {

                GMapMarker marker = new GMarkerGoogle(
                    new PointLatLng(l.Latitude, l.Longitude),
                    GMarkerGoogleType.blue_pushpin
                    );
                marker.ToolTipText = l.NameOfCoord + " " + l.Date;
                marker.Tag = l.NameOfCoord;
                marker.ToolTip.TextPadding = new Size(10, 10);
                markers.Markers.Add(marker);


            }
            gmap.Overlays.Add(markers);
            //

        }

        //for testing purpose
        private void gmap_OnMarkerClick(GMapMarker item, MouseEventArgs e)
        {
            notes.Text = (String.Format("{0}", item.Tag));
        }

        private void GetLocationData() 
        {
            String url = @"http://developer.kensnz.com/getlocdata";
            using (WebClient client = new WebClient()) 
            {                
                var json = client.DownloadString(url);
                
                JavaScriptSerializer ser = new JavaScriptSerializer();
                var JSONArray = ser.Deserialize<Dictionary<string, string>[]>(json);
                foreach (Dictionary<string, string> map in JSONArray)
                {
                    int userid = int.Parse(map["userid"]);
                    double latitude = double.Parse(map["latitude"]);
                    double longitude = double.Parse(map["longitude"]);
                    string description = map["description"];
                    string date = map["updated_at"].Split(' ')[0];
                    if ( date == "2021-11-21")                 
                    {
                        Location loc = new Location(userid, description, latitude, longitude, date);
                        locationsList.Add(loc);
                    }
                   
                    
                 
                }

              
            }
           
            locationsList.RemoveRange(0, 5);// remove the dummy data which have recorded at the start 
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
           

        }

        private void gmap_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.H)
            {
                if (showHull) 
                {
                    showHull = false;
                    locationsList = locationsListBank.ToList();
                    gmap.Overlays.RemoveAt(1);
                }
                else {
                    showHull = true;
                    //covex hull operation
                    locationsListBank = locationsList.ToList();
                    locationsList = locationsList.Distinct(new EqualityComparer()).ToList();
                    locationsList.Sort(new LocationComparer());
                    pivotLocation = locationsList[0];
                    locationsList.RemoveAt(0);

                    //locationsList.Sort(new RadialSort(pivotLocation));
                    for (int i = 0; i < locationsList.Count; i++)
                    {
                        for (int j = i + 1; j < locationsList.Count; j++)
                        {
                            if (-RadialSort.WorkOutArea(pivotLocation, locationsList[i], locationsList[j]) > 0)
                            {
                                Location l = locationsList[i];
                                locationsList[i] = locationsList[j];
                                locationsList[j] = l;
                            }
                        }
                    }

                    locationsList.Insert(0, pivotLocation);
                   

                    List<Location> covexHull = FindCovexHullUsingList(locationsList);
                    // covex hull operation ends

                    GMapOverlay polygons = new GMapOverlay("polygons");
                    List<PointLatLng> points = new List<PointLatLng>();
                    foreach (Location l in covexHull)
                    {
                        points.Add(new PointLatLng(l.Latitude, l.Longitude));
                    }
                    GMapPolygon polygon = new GMapPolygon(points, "jfdkdkf");
                    polygons.Polygons.Add(polygon);
                    gmap.Overlays.Add(polygons);
                    
                }
                //refresh the map
                gmap.Zoom -= 0.1;
                gmap.Zoom += 0.1;



          
            }
           
        }

        static List<Location> FindCovexHullUsingList(List<Location> locationList) 
        {
            List<Location> covexHull = new List<Location>();//locationList.ToList();
            covexHull.Add(locationList[0]);
            locationList.RemoveAt(0);
            covexHull.Add(locationList[0]);
            locationList.RemoveAt(0);
            covexHull.Add(locationList[0]);
            locationList.RemoveAt(0);
            while (locationList.Count>0) {
                while (RadialSort.WorkOutArea(covexHull[covexHull.Count - 2], covexHull[covexHull.Count - 1], locationList[0]) <= 0) {
                    covexHull.RemoveAt(covexHull.Count - 1);
                }
                covexHull.Add(locationList[0]);
                locationList.RemoveAt(0);
            }
            return covexHull;
        }

    }

    class Location 
    {
        public int UserID { get; set; }
        public double Latitude { get; set; }

        public double Longitude { get; set; }
        public string NameOfCoord { get; set; }
        public string Date { get; set; }
        public Location(int userID, string nameOfCoord, double latitude, double longitude, string date)
        {
            UserID = userID;
            NameOfCoord = nameOfCoord;
            Latitude = latitude;
            Longitude = longitude;
            Date = date;
        }
    }

    //this class is used to removed duplicates
    class EqualityComparer : IEqualityComparer<Location>
    {
        public bool Equals(Location a, Location b)
        {
            return (a.Longitude == b.Longitude && a.Latitude == b.Latitude);
        }

        public int GetHashCode(Location obj)
        {
            return (obj.Latitude * obj.Longitude).GetHashCode();
        }
    }

    // this class is used to find lower left point
    class LocationComparer : IComparer<Location>
    {
        
        public int Compare(Location first, Location second) 
        {

            if (first.Latitude == second.Latitude)
            {
                if (first.Longitude == second.Longitude) return 0;
                else if (first.Longitude < second.Longitude) return -1;
                else return 1;
            }
            else if (first.Latitude < second.Latitude)
            {
                return -1;
            }
            else {
                return 1;
            }
            
        }
    }

    // This class is used for radial sort in covex hull operation
    class RadialSort : IComparer<Location> 
    {
        public Location PivotLocation { get; set; }
        public RadialSort(Location pivotLocation)
        {
            PivotLocation = pivotLocation;
        }

        public static double WorkOutArea(Location p, Location b, Location c) 
        {
            return 0.5*(p.Latitude * b.Longitude - p.Latitude * c.Longitude - p.Longitude * b.Latitude + p.Longitude * c.Latitude + b.Latitude * c.Longitude - b.Longitude * c.Latitude);
        }
        public int Compare(Location b, Location c) 
        {
            double x = -WorkOutArea(PivotLocation, b, c);
            if (x < 0)  return -1; 
            else if (x > 0)  return 1; 
            else  return 0; 
        }
    }
}
