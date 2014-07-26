using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CatgirlTech
{
    class CgTResourceManager : PartModule
    {
        public List<ManagedResource> resources = new List<ManagedResource>();
        //[KSPField(isPersistant = true)]
        public List<ManagedPart> managed_parts = new List<ManagedPart>();
        [KSPField(isPersistant = true)]
        public string managed_resources_string;
        [KSPField(isPersistant = true)]
        public string managed_parts_string;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Part Count")]
        public uint managed_parts_count;

        private bool showGui = true;


        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            // my test codes
            ScreenMessages.PostScreenMessage("Hello?!", 6, ScreenMessageStyle.UPPER_CENTER);
            //if (state == StartState.Editor) { return; }
            for (int p = 0; p < this.vessel.parts.Count; p++)
            {
                if (this.vessel.parts[p].Resources.Count > 0)
                {
                    // add part
                    managed_parts.Add(new ManagedPart(this.vessel.parts[p]));

                    for (int i = 0; i < this.vessel.parts[p].Resources.Count; i++)
                    {
                        if (this.part != this.vessel.parts[p])
                        {
                            ManagedResource mr = new ManagedResource(this.vessel.parts[p].Resources[i]);

                            bool isNew = true;
                            for (int r = 0; r < resources.Count; r++)
                            {
                                if (resources[r].name == mr.name)
                                {
                                    isNew = false;
                                }
                            }
                            if (isNew)
                                resources.Add(mr);

                        }

                    }

                }

            }
            print("manged resources: " + managed_resources_string);
            if (managed_resources_string != "")
            {
                // load resources managed.
                for (int i = 0; i < resources.Count; i++)
                {
                    resources[i].managed = (managed_resources_string.IndexOf(resources[i].name) > -1);
                }
            } else
                managed_resources_string = resourcesString();

            print("manged parts: " + managed_parts_string);
            if (managed_parts_string != "")
            {
                string[] parts_index_str = managed_parts_string.Split(',');
                for (int i = 0; i < parts_index_str.Length; i++)
                {
                    print("managed part: " + parts_index_str[i]);
                    managed_parts[int.Parse(parts_index_str[i])].managed = true;
                }
            } else
                managed_parts_string = partsString();
            onPartStart();
            //onFlightStart();
            //updateResources();
        }

        //
        // save stuff
        //
        private string resourcesString()
        {
            
            string str = "";
            int mi = 0;
            for (int i = 0; i < resources.Count; i++)
            {
                if (resources[i].managed)
                {
                    if (mi > 0)
                        str += ", ";
                    str += resources[i].name;
                    mi++;
                }
            }
            return str;
        }
        private string partsString()
        {
            string str = "";
            managed_parts_count = 0;
            for (int i = 0; i < managed_parts.Count; i++)
            {
                if (managed_parts[i].managed)
                {
                    if (managed_parts_count > 0)
                        str += ", ";
                    str += i.ToString();
                    managed_parts_count++;
                }
            }
            return str;
            
        }
        public override void OnUpdate()
        {
            base.OnUpdate();

            for (int i = 0; i < managed_parts.Count; i++)
            {
                if (managed_parts[i].part == null || !managed_parts[i].part.isAttached || !managed_parts[i].part.isConnected)
                {
                    managed_parts.RemoveAt(i);
                }
            }

            managed_resources_string = resourcesString();
            managed_parts_string = partsString();
            updateResources();

        }

        [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "Update Resource List", active = true)]
        private void updateResources()
        {
            for (int i = 0; i < resources.Count; i++)
            {
                PartResource res = getResource(part, resources[i].name);
                if (res == null && resources[i].managed)
                {
                    ConfigNode node = new ConfigNode("RESOURCE");
                    node.AddValue("name", resources[i].name);
                    node.AddValue("amount", 0);
                    node.AddValue("maxAmount", 1);
                    res = part.AddResource(node);
                }
                else if (res != null && !resources[i].managed)
                {
                    part.Resources.list.Remove(res);
                }
                else if (res != null && resources[i].managed)
                {
                    // here's where we can do the stuff where we keep it at 0.5
                    if (managed_parts_count > 0 && (res.amount < 0.2 || res.amount > 0.8)){
                        // push res out to a managed part
                        internalResourceTransfer(res, 0.5 - res.amount);
                    }
                }
            }

        }

        // internal transfer
        private void internalResourceTransfer(PartResource res, double amount)
        {
            for (int i = 0; i < managed_parts.Count; i++)
            {
                if (managed_parts[i].managed)
                {
                    PartResource res_internal = getResource(managed_parts[i].part, res.resourceName);
                    var res_def = PartResourceLibrary.Instance.GetDefinition(res.resourceName);
                    if (res_internal != null)
                    {
                        double amount_result = managed_parts[i].part.TransferResource(res_def.id, amount*-1);
                        if (amount_result != 0)
                        {
                            part.TransferResource(res_def.id, amount_result);
                            break;
                        }
                    }
                }
            }
        }


        public static PartResource getResource(Part part, string name)
        {
            PartResourceList resourceList = part.Resources;
            return resourceList.list.Find(delegate(PartResource cur)
            {
                return (cur.resourceName == name);
            });
        }


        // toggle gui
        [KSPEvent(guiActive = true,guiActiveEditor=true, guiName = "Toggle GUI", active = true)]
        public void toggleGui()
        {
            if (showGui)
            {
                RenderingManager.AddToPostDrawQueue(3, new Callback(drawGUI));//start the GUI
            }
            else
            {
                RenderingManager.RemoveFromPostDrawQueue(3, new Callback(drawGUI)); //close the GUI
            }
            showGui = !showGui;

        }
        //[KSPAction("Toggle GUI")]
        public void toggleGuiAction(KSPActionParam param)
        {
            toggleGui();
        }
        
        

        //
        //
        // gui window code
        //
        //
        protected Rect windowPos;

        private uint guiWindowMode = 0;
        private Vector2 scrollPosition;
        private bool blah;

        private void WindowGUI(int windowID)
        {
            // button style
            GUIStyle mySty = new GUIStyle(GUI.skin.button);
            mySty.normal.textColor = mySty.focused.textColor = Color.white;
            mySty.hover.textColor = mySty.active.textColor = Color.yellow;
            mySty.onNormal.textColor = mySty.onFocused.textColor = mySty.onHover.textColor = mySty.onActive.textColor = Color.green;
            mySty.padding = new RectOffset(8, 8, 8, 8);
            // header style
            GUIStyle hdSty = new GUIStyle(GUI.skin.label);
            hdSty.normal.textColor = hdSty.focused.textColor = Color.green;
            hdSty.fontSize = 16;
            hdSty.fontStyle = FontStyle.Bold;
            hdSty.padding = new RectOffset(8, 8, 8, 8);

            // scroll area options


            GUILayout.BeginVertical();

            switch (guiWindowMode)
            {
                case 1:
                    if (GUILayout.Button("Managed Parts", mySty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                    {
                        guiWindowMode = 2;
                    }
                    // managed resources gui window goes here
                    GUILayout.Label("Managed Resource:", hdSty, GUILayout.ExpandWidth(true));
                    scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true,GUILayout.Height(200));
                    GUILayout.BeginVertical();
                    for (int i = 0; i < resources.Count(); i++)
                    {
                        resources[i].managed = GUILayout.Toggle(resources[i].managed, resources[i].name);
                        if (Event.current.type == EventType.Repaint &&
                           GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                        {
                            managed_resources_string = resourcesString();
                        }
                    }
                    GUILayout.EndVertical();
                    GUILayout.EndScrollView();
                    GUILayout.Label("select resources to manage. the types of resources that the manager will handle.");
                    break;
                case 2:
                    if (GUILayout.Button("Managed Resources", mySty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                    {
                        guiWindowMode = 1;
                    }
                    // managed part gui window goes here
                    GUILayout.Label("Managed Parts:", hdSty, GUILayout.ExpandWidth(true));
                    scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true,GUILayout.Height(200));
                    GUILayout.BeginVertical();
                    for (int i = 0; i < managed_parts.Count(); i++)
                    {
                        managed_parts[i].managed = GUILayout.Toggle(managed_parts[i].managed, managed_parts[i].part.partInfo.title);
                        if (Event.current.type == EventType.Repaint &&
                           GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                        {
                            managed_parts[i].part.SetHighlight(true);
                        }
                        else
                        {
                            managed_parts[i].part.SetHighlight(false);
                        }
                    }
                    GUILayout.EndVertical();
                    GUILayout.EndScrollView();
                    GUILayout.Label("select parts to manage. what parts it will manage resources for.");
                    break;
                case 0:
                    // select the
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Managed Resources", mySty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                    {
                        guiWindowMode = 1;
                    }
                    if (GUILayout.Button("Managed Parts", mySty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                    {
                        guiWindowMode = 2;
                    }
                    GUILayout.EndHorizontal();
                    break;
            }
            if (GUILayout.Button("Close", mySty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
            {
                RenderingManager.RemoveFromPostDrawQueue(3, new Callback(drawGUI)); //close the GUI
                showGui = true;
            }
            GUILayout.EndVertical();

            //DragWindow makes the window draggable. The Rect specifies which part of the window it can by dragged by, and is 
            //clipped to the actual boundary of the window. You can also pass no argument at all and then the window can by
            //dragged by any part of it. Make sure the DragWindow command is AFTER all your other GUI input stuff, or else
            //it may "cover up" your controls and make them stop responding to the mouse.
            GUI.DragWindow(new Rect(0, 0, 10000, 20));

        }
        private void drawGUI()
        {
            GUI.skin = HighLogic.Skin;
            windowPos = GUILayout.Window(1, windowPos, WindowGUI, "Resource Manager", GUILayout.MinWidth(200));
        }
        protected void onFlightStart()  //Called when vessel is placed on the launchpad
        {
            RenderingManager.AddToPostDrawQueue(3, new Callback(drawGUI));//start the GUI
        }
        protected void onPartStart()
        {
            if ((windowPos.x == 0) && (windowPos.y == 0))//windowPos is used to position the GUI window, lets set it in the center of the screen
            {
                windowPos = new Rect(Screen.width / 2, Screen.height / 2, 10, 10);
            }
        }
        protected void onPartDestroy()
        {
            RenderingManager.RemoveFromPostDrawQueue(3, new Callback(drawGUI)); //close the GUI
        }
        public override void OnInactive()
        {
            base.OnInactive();
            onPartDestroy();
        }
        
        
        
        //
        //
        // member classes?
        //
        //
        
        public class ManagedResource
        {
            // data
            public bool managed;
            public string name;
            public int resourceID;
            // constructor
            public ManagedResource(PartResource resource, bool _managed = false)
            {
                name = resource.resourceName;
                managed = _managed;
            }
            // to string
            public override string ToString()
            {
                return name;
            }
        }

        public class ManagedPart
        {
            // data
            public bool managed;
            public Part part;

            // constructor
            public ManagedPart(Part _part, bool _managed = false)
            {
                part = _part;
                managed = _managed;
            }
            
            // to string
            public override string ToString()
            {
                return part.partInfo.title;
            }
        }
        
    }
}
