using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CatgirlTech
{
    class CgTResourceManager : PartModule
    {
        [KSPField(isPersistant = true)]
        public float maxManagedResources;
        [KSPField(isPersistant = true)]
        public float maxManagedParts;

        public List<ManagedResource> resources = new List<ManagedResource>();
        //[KSPField(isPersistant = true)]
        public List<ManagedPart> managed_parts = new List<ManagedPart>();
        [KSPField(isPersistant = true)]
        public string managed_resources_string;
        [KSPField(isPersistant = true)]
        public string managed_parts_string;
        [KSPField(isPersistant = true)]
        public bool fastIntake = true;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Parts")]
        public uint managed_parts_count;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Resources")]
        public uint managed_resource_count;

        // can show up to 10 resource types in this list?
        [KSPField(isPersistant = false, guiActive = false, guiName = "Resource1")]
        public string resource_total_amount_display_1;
        [KSPField(isPersistant = false, guiActive = false, guiName = "Resource2")]
        public string resource_total_amount_display_2;
        [KSPField(isPersistant = false, guiActive = false, guiName = "Resource3")]
        public string resource_total_amount_display_3;
        [KSPField(isPersistant = false, guiActive = false, guiName = "Resource4")]
        public string resource_total_amount_display_4;
        [KSPField(isPersistant = false, guiActive = false, guiName = "Resource5")]
        public string resource_total_amount_display_5;
        [KSPField(isPersistant = false, guiActive = false, guiName = "Resource6")]
        public string resource_total_amount_display_6;
        [KSPField(isPersistant = false, guiActive = false, guiName = "Resource7")]
        public string resource_total_amount_display_7;
        [KSPField(isPersistant = false, guiActive = false, guiName = "Resource8")]
        public string resource_total_amount_display_8;
        [KSPField(isPersistant = false, guiActive = false, guiName = "Resource9")]
        public string resource_total_amount_display_9;
        [KSPField(isPersistant = false, guiActive = false, guiName = "Resource10")]
        public string resource_total_amount_display_10;


        private bool showGui = true;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            Fields["managed_parts_count"].guiUnits = "/" + maxManagedParts;
            Fields["managed_resource_count"].guiUnits = "/" + maxManagedResources;
            // update the available resources.
            upateAvailabeResourcesList();

            
            // load in persistent data
            //print("manged resources: " + managed_resources_string);
            if (managed_resources_string != "")
            {
                // load resources managed.
                for (int i = 0; i < resources.Count; i++)
                {
                    resources[i].managed = (managed_resources_string.IndexOf(resources[i].name) > -1);
                }
            } else
                managed_resources_string = resourcesString();

            //print("manged parts: " + managed_parts_string);
            if (managed_parts_string != "")
            {
                string[] parts_index_str = managed_parts_string.Split(',');
                for (int i = 0; i < parts_index_str.Length; i++)
                {
                   // print("managed part: " + parts_index_str[i]);
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
            managed_resource_count = 0;
            for (int i = 0; i < resources.Count; i++)
            {
                if (resources[i].managed)
                {
                    if (managed_resource_count > 0)
                        str += ", ";
                    str += resources[i].name;
                    managed_resource_count++;
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

            // update totals display
            int ri = 1;
            for (int i = 0; i < resources.Count; i++)
            {
                if (resources[i].managed)
                {
                    Fields["resource_total_amount_display_" + ri].guiName = resources[i].name;
                    setResourceTotalAmountDisplay(ri, Math.Round(getResourceAmount(resources[i].name)) + "/" + Math.Round(getResourceMaxAmount(resources[i].name)));
                    Fields["resource_total_amount_display_" + ri].guiActive = true;
                    ri++;
                }
            }
            for (int i = ri; i <= 10; i++)
            {
                Fields["resource_total_amount_display_" + ri].guiActive = false;
            }
            updateFreeSpace();
        }
        private void setResourceTotalAmountDisplay(int i, string display_amount)
        {
            switch (i)
            {
                case 1:
                    resource_total_amount_display_1 = display_amount;
                    break;
                case 2:
                    resource_total_amount_display_2 = display_amount;
                    break;
                case 3:
                    resource_total_amount_display_3 = display_amount;
                    break;
                case 4:
                    resource_total_amount_display_4 = display_amount;
                    break;
                case 5:
                    resource_total_amount_display_5 = display_amount;
                    break;
                case 6:
                    resource_total_amount_display_6 = display_amount;
                    break;
                case 7:
                    resource_total_amount_display_7 = display_amount;
                    break;
                case 8:
                    resource_total_amount_display_8 = display_amount;
                    break;
                case 9:
                    resource_total_amount_display_9 = display_amount;
                    break;
                case 10:
                    resource_total_amount_display_10 = display_amount;
                    break;
            }
        }
     

        //[KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "Update Resource List", active = true)]
        private void updateResources()
        {
            upateAvailabeResourcesList();
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
                    if (res.amount > 0)
                    {
                        internalResourceTransfer(res, res.amount*-1);
                    }
                    part.Resources.list.Remove(res);
                }
                else if (res != null && resources[i].managed)
                {
                    //
                    // handle resource balancing
                    //
                    PartResource freeSpace = getResource(this.part, "InternalTransferSpace");
                    // fast intake
                    if (fastIntake)
                    {
                        double available_space = getResourceMaxAmount(res.resourceName) - getResourceAmount(res.resourceName) + freeSpace.maxAmount / (managed_resource_count + 1);
                        if (available_space < freeSpace.maxAmount / (managed_resource_count + 1) )
                            available_space = freeSpace.maxAmount / (managed_resource_count + 1);
                        if (available_space > res.amount)
                        {
                            res.maxAmount = available_space;
                        }
                    }
                    else
                        res.maxAmount = 1;
                    
                    
                    // here's where we can do the stuff where we keep it at 0.5
                    if (managed_parts_count > 0 && (res.amount == res.maxAmount || res.amount < 0.2 || res.amount > freeSpace.maxAmount / (managed_resource_count + 1) - 0.1))
                    {
                        // push res out to a managed part
                        internalResourceTransfer(res, 0.5 - res.amount);
                    }
                    

                }
            }

        }

        // update the available resources list
        private void upateAvailabeResourcesList()
        {
            for (int p = 0; p < this.vessel.parts.Count; p++)
            {
                if (this.vessel.parts[p].Resources.Count > 0)
                {
                    // add part
                    if (this.part != this.vessel.parts[p])
                    {
                        bool newPart = true;
                        for (int np = 0; np < managed_parts.Count; np++)
                        {
                            if (managed_parts[np].part == this.vessel.parts[p])
                            {
                                newPart = false;
                            }
                        }

                        if (newPart)
                        {
                            managed_parts.Add(new ManagedPart(this.vessel.parts[p]));

                            for (int i = 0; i < this.vessel.parts[p].Resources.Count; i++)
                            {

                                PartResourceDefinition res_def = PartResourceLibrary.Instance.GetDefinition(this.vessel.parts[p].Resources[i].resourceName);
                                if (res_def.resourceTransferMode == ResourceTransferMode.PUMP)
                                {
                                    if (res_def.density > 0)
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

                    }

                }

            }

        }

        private void updateFreeSpace()
        {
            // freeSpace represents the real space that resources can take up.
            PartResource freeSpace = getResource(this.part, "InternalTransferSpace");
            freeSpace.amount = freeSpace.maxAmount;
            for (int i = 0; i < resources.Count; i++)
            {
                if (resources[i].managed)
                {
                    PartResource mRes = getResource(this.part, resources[i].name);
                    var res_def = PartResourceLibrary.Instance.GetDefinition(resources[i].name);
                    freeSpace.amount -= mRes.amount;
                }
            }

        }
        // internal transfer
        private void internalResourceTransfer(PartResource res, double amount)
        {
            
            updateFreeSpace();
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
            updateFreeSpace();
        }

        //
        // return the amount of a resource available among all managed parts and any inside this part.
        //
        private double getResourceAmount(string resource_name)
        {
            double amount = 0.0;
            if (managed_parts_count != 0)
            {
                for (int i = 0; i < managed_parts.Count; i++)
                {
                    if (managed_parts[i].managed)
                    {
                        PartResource res = getResource(managed_parts[i].part, resource_name);
                        if (res != null)
                        {
                            amount += res.amount;
                        }
                    }
                }
            }
            PartResource res_me = getResource(part, resource_name);
            if (res_me != null)
            {
                amount += res_me.amount;
            }
            return amount;
        }
        //
        // return the amount of a resource available among all managed parts and any inside this part.
        //
        private double getResourceMaxAmount(string resource_name)
        {
            double amount = 0.0;
            if (managed_parts_count != 0)
            {
                for (int i = 0; i < managed_parts.Count; i++)
                {
                    if (managed_parts[i].managed)
                    {
                        PartResource res = getResource(managed_parts[i].part, resource_name);
                        if (res != null)
                        {
                            amount += res.maxAmount;
                        }
                    }
                }
            }
            return amount;
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
        [KSPEvent(guiActive = true, guiName = "Show Setup", active = true)]
        public void toggleGui()
        {
            if (showGui)
            {
                RenderingManager.AddToPostDrawQueue(3, new Callback(drawGUI));//start the GUI
                Events["toggleGui"].guiName = "Hide Setup";
            }
            else
            {
                RenderingManager.RemoveFromPostDrawQueue(3, new Callback(drawGUI)); //close the GUI
                Events["toggleGui"].guiName = "Show Setup";
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
            // header style
            GUIStyle nSty = new GUIStyle(GUI.skin.label);
            nSty.normal.textColor = hdSty.focused.textColor = Color.yellow;
            nSty.fontSize = 10;

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
                    GUILayout.Label("Select up to "+maxManagedResources);
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
                    GUILayout.Label("Select up to "+maxManagedParts);
                    scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true,GUILayout.Height(200));
                    GUILayout.BeginVertical();
                    for (int i = 0; i < managed_parts.Count(); i++)
                    {
                        if (managed_parts[i].hasManagedResources(resources))
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
            fastIntake = GUILayout.Toggle(fastIntake, "Fast Intake*");
            if(fastIntake)
                GUILayout.Label("*This will in effect double the vessel's max amount for managed resources in the vessel's resources viewer. It does however dramaticly increase the transfer rate.",nSty);
            if (GUILayout.Button("Close", mySty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
            {
                RenderingManager.RemoveFromPostDrawQueue(3, new Callback(drawGUI)); //close the GUI
                showGui = true;
                Events["toggleGui"].guiName = "Show Setup";
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
            Events["toggleGui"].guiName = "Show Setup";
            showGui = false;
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
            public double amount;
            public double maxAmount;
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

            public bool hasManagedResources(List<ManagedResource> resources)
            {
                foreach (ManagedResource r in resources)
                {
                    if (r.managed && getResource(part, r.name))
                    {
                        return true;
                    }
                }

                return false;
            }
            
            // to string
            public override string ToString()
            {
                return part.partInfo.title;
            }
        }
        
    }
}
