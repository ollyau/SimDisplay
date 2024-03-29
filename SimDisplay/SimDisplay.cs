﻿using BeatlesBlog.SimConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SimDisplay {
    class SimDisplay {
        /// <summary>
        /// Gets assembly load directory
        /// (typically the directory the program executable is in)
        /// </summary>
        string AssemblyLoadDirectory {
            get {
                return AppDomain.CurrentDomain.BaseDirectory;
            }
        }

        /// <summary>
        /// Checks if a string has invalid file name characters.
        /// </summary>
        /// <param name="input">String to check.</param>
        /// <returns>True if string is invalid, otherwise false.</returns>
        bool InvalidFileNameChars(string input) {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars()) {
                if (input.Contains(c)) { return true; }
            }
            return false;
        }

        /// <summary>
        /// Checks if a string has invalid file path characters.
        /// </summary>
        /// <param name="input">String to check.</param>
        /// <returns>True if string is invalid, otherwise false.</returns>
        bool InvalidPathChars(string input) {
            foreach (char c in System.IO.Path.GetInvalidPathChars()) {
                if (input.Contains(c)) { return true; }
            }
            return false;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="settingsFilename">Optional path to SimDisplay settings file.</param>
        public SimDisplay(string settingsFilename = "SimDisplay.xml") {
            // set settings file path
            if (!System.IO.Path.IsPathRooted(settingsFilename)) {
                // assume it's relative to the exe directory
                if (InvalidFileNameChars(settingsFilename)) {
                    throw new ArgumentException("Invalid filename.");
                }
                else {
                    Initialize(System.IO.Path.Combine(AssemblyLoadDirectory, settingsFilename));
                }
            }
            // else if(uri is valid && site can connected){call initialize, add if statement there to load from website} add condition for xml file on website
            else {
                // otherwise it's a full path
                if (InvalidPathChars(settingsFilename)) {
                    throw new ArgumentException("Invalid file path.");
                }
                else {
                    Initialize(settingsFilename);
                }
            }
        }

        private void Initialize(string settingsFilePath) {
            // load settings
            Settings s;
            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            if (System.IO.File.Exists(settingsFilePath)) {
#if !DEBUG
                try {
#endif
                using (System.IO.Stream reader = new System.IO.FileStream(settingsFilePath, System.IO.FileMode.Open)) {
                    s = (Settings)serializer.Deserialize(reader);
                }
#if !DEBUG
                }
                catch (Exception ex) {
                    Console.WriteLine("Unable to load settings XML file:\r\n\r\n{0}\r\n\r\n{1}", settingsFilePath, ex.Message);
                    return;
                }
#endif
            }
            else {
                // create and save settings if no file exists
                s = new Settings();
                s.Objects.Add(new SimObject("veh_carrier01_high_detail_sm", 37.793322, -122.335726, 0, 0, 0, 0, true, 0, false, LoadTypes.Immediate));
                s.Objects.Add(new SimObject("VEH_water_yacht_280ft_sm", 37.787659, -122.333902, 0, 0, 0, 0, true, 0, false, LoadTypes.Immediate));
                s.Objects.Add(new SimObject("VEH_Air_PickupUS_Grey_sm", 37.790607, -122.330697, 0, 0, 0, 0, true, 0, false, LoadTypes.Immediate));
                s.Objects.Add(new SimObject("Discovery Spaceshuttle", 37.790486, -122.328211, 0, 0, 0, 0, true, 0, false, LoadTypes.Immediate));
                s.Objects.Add(new SimObject("Robinson R22", 37.790314, -122.330437, 0, 0, 0, 0, true, 0, false, LoadTypes.Immediate));
                s.Objects.Add(new SimObject("ANI_GiraffeWalk_Mature_sm", 37.790338, -122.329498, 0, 0, 0, 0, true, 0, false, LoadTypes.Immediate));
                s.Objects.Add(new SimObject("Cessna Skyhawk 172SP Paint1", 37.790615, -122.330056, 0, 0, 0, 0, true, 0, false, LoadTypes.Immediate));

                s.Objects[0].WaypointList = new List<Waypoint>();
                s.Objects[0].WaypointList.Add(new Waypoint(
                    37.805422,
                    -122.353511,
                    0,
                    SIMCONNECT_WAYPOINT_FLAGS.ALTITUDE_IS_AGL | SIMCONNECT_WAYPOINT_FLAGS.ON_GROUND | SIMCONNECT_WAYPOINT_FLAGS.SPEED_REQUESTED,
                    30
                    ));

                s.Objects[0].WaypointList.Add(new Waypoint(
                    37.847885,
                    -122.354810,
                    0,
                    SIMCONNECT_WAYPOINT_FLAGS.ALTITUDE_IS_AGL | SIMCONNECT_WAYPOINT_FLAGS.ON_GROUND | SIMCONNECT_WAYPOINT_FLAGS.SPEED_REQUESTED,
                    30
                    ));

                s.Objects[0].WaypointList.Add(new Waypoint(
                    37.832973, -122.401158,
                    0,
                    SIMCONNECT_WAYPOINT_FLAGS.ALTITUDE_IS_AGL | SIMCONNECT_WAYPOINT_FLAGS.ON_GROUND | SIMCONNECT_WAYPOINT_FLAGS.SPEED_REQUESTED,
                    30
                    ));

                s.Objects[0].WaypointList.Add(new Waypoint(
                    37.781164, -122.365796,
                    0,
                    SIMCONNECT_WAYPOINT_FLAGS.ALTITUDE_IS_AGL | SIMCONNECT_WAYPOINT_FLAGS.ON_GROUND | SIMCONNECT_WAYPOINT_FLAGS.SPEED_REQUESTED,
                    30
                    ));

                using (System.IO.Stream writer = new System.IO.FileStream(settingsFilePath, System.IO.FileMode.Create)) {
                    serializer.Serialize(writer, s);
                }
            }

            // create objects that should be loaded immediately
            SimConnectInstance sc = SimConnectInstance.Instance;
            if (sc.Connected) {
                foreach (SimObject obj in s.Objects.Where(x => x.LoadType == LoadTypes.Immediate)) {
                    sc.AICreateSimulatedObject(obj);
                }
            }
        }
    }
}
