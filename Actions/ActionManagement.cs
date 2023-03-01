using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FileManagement;

namespace ActionManagement
{
    public static class ActionManager
    {
        // BLOCK STUFF
        public readonly static int BLOCK_CLICKED = 1;
        public readonly static int PLACE_SELECT = 2; // mode
        public readonly static int DELETE_SELECT = 3; // mode
        public readonly static int INSERT_LINE = 4; // mode
        public readonly static int CREATE_NAME = 5; // mode
        public readonly static int NAME_NEW_F_M_V = 6;
        public readonly static int NAME_VARIABLE = 7;

        // TOOLS
        public readonly static int SAVE = 10;
        public readonly static int COMPILE = 11;

        // WINDOW STUFF
        public readonly static int OPEN_WORKSPACE = 21;
        public readonly static int CREATE_WORKSPACE = 22;
        public readonly static int OPEN_FILE = 23;
        public readonly static int CREATE_FILE = 24;
        public readonly static int CREATE_SNIPPET = 25;
        public readonly static int BACK_TO_WORKSPACES = 26;



        private static Dictionary<int, Act> actions = null;
        private static void initialiseActions()
        {
            actions = new Dictionary<int, Act>();

            actions.Add(BLOCK_CLICKED, new BlockClicked());
            actions.Add(PLACE_SELECT, new Place());
            actions.Add(DELETE_SELECT, new Delete());
            actions.Add(INSERT_LINE, new InsertLine());
            actions.Add(CREATE_NAME, new CreateName());
            actions.Add(NAME_NEW_F_M_V, new NameNewFMV());
            actions.Add(NAME_VARIABLE, new NameVariable());

            //actions.Add(SAVE, new SaveCode());
            //actions.Add(COMPILE, new Compile());

            //actions.Add(OPEN_WORKSPACE, new OpenWorkspace());
            //actions.Add(CREATE_WORKSPACE, new CreateWorkspace());
            //actions.Add(OPEN_FILE, new OpenFile());
            actions.Add(CREATE_FILE, new CreateFile());
            actions.Add(CREATE_SNIPPET, new CreateSnippet());
            //actions.Add(BACK_TO_WORKSPACES, new BackToWorkspaces());
        }



        private static Mode currentMode = null;
        private static void setCurrentMode(Mode mode, object data)
        {
            string toolsWindowMessage = "";


            // if this is an already active mode
            if (mode == currentMode)
            {
                if (mode == null) return;


                try
                {
                    // if this mode can selected repeatedly
                    // (and is called outside of ActionManager)
                    if (mode.multiSelect)
                    {
                        mode.onSelect(data);
                        toolsWindowMessage = mode.getToolsWindowMessage();
                    }
                    else
                    {
                        mode.onCall(data);
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("Err calling mode.");
                    //Debug.Log(e.StackTrace);
                    InspectionPlatform.Log(e.StackTrace);

                    currentMode = null;
                    toolsWindowMessage = "ERR MODE CALL";
                }
            }


            // if this a new mode
            else
            {
                if (currentMode != null) currentMode.onDeselect(); // deactivate current mode

                currentMode = mode; // switch state

                try
                {
                    if (mode != null)
                    {
                        // activate mode (if exists)
                        mode.onSelect(data);
                        toolsWindowMessage = mode.getToolsWindowMessage();
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("Err selecting mode.");
                    //Debug.Log(e.StackTrace);
                    InspectionPlatform.Log(e.StackTrace);

                    currentMode = null;
                    toolsWindowMessage = "ERR MODE SELECT";
                }
            }


            // set message on toolsWindow
            TitledWindow toolsWindow = (TitledWindow)WindowManager.getWindowWithName("ToolsWindow");
            if (toolsWindow != null)
                toolsWindow.setTitleMessage(toolsWindowMessage);
        }
        public static bool callCurrentMode(object data)
        {
            try
            {
                currentMode.onCall(data);
                return true;
            }
            catch (Exception e)
            {
                Debug.Log("Err calling mode.");
                //Debug.Log(e.StackTrace);
                InspectionPlatform.Log(e.StackTrace);
                return false;
            }
        }
        public static void clearMode()
        {
            setCurrentMode(null, null);
        }
        public static int getCurrentModeIndex()
        {
            foreach (char c in actions.Keys)
                if (actions[c] == currentMode) return c;
            return 0;
        }



        public static void callAction(int action, object data)
        {
            if (actions == null) initialiseActions();

            if (action == 0)
            {
                clearMode();
                return;
            }
            else if (!actions.ContainsKey(action))
            {
                Debug.Log("Action " + action + " was not recognised.");
                return;
            }

            Act newAction = actions[action];





            // is this newAction a mode-selection
            if (newAction is Mode mode)
            {
                setCurrentMode(mode, data);
            }
            // or immediate action
            else
            {
                try { newAction.onCall(data); }
                catch (Exception e)
                {
                    Debug.Log("Err calling action " + action + " with data: " + data);
                    //Debug.Log(e.StackTrace);
                    InspectionPlatform.Log(e.StackTrace);
                }
            }
        }



        public static string stringToSafeName(string str)
        {
            string name = "";
            foreach (char c in str)
            {
                if (c == '_') ;
                else if (c >= 48 && c <= 57) ; // numbers
                else if (c >= 65 && c <= 90) ; // uppercase
                else if (c >= 97 && c <= 122) ; // lowercase
                else continue;

                name += c;
            }

            return name;
        }
    }



    public interface Act
    {
        public void onCall(object data);
    }



    public abstract class Mode : Act
    {
        public bool multiSelect = false;

        public abstract void onCall(object data);

        public abstract void onSelect(object data);

        public abstract void onDeselect();

        public abstract string getToolsWindowMessage();
    }



    public class BlockClicked : Act
    {
        public void onCall(object data)
        {
            int modeIndex = ActionManager.getCurrentModeIndex();

            Block clicked = (Block)data;
            Block master = clicked.getMasterBlock();
            BlockManager.BlockVariant variant = clicked.getBlockVariant();
            string type = variant.getBlockType();



            // check for special types first

            // cycleable blocks go to next state
            if (BlockManager.isCycleable(type))
            {
                int nVIndex = BlockManager.cycleBlockVariantIndex(variant);
                // change block variant to next in cycle and enable colliders
                Block spawned = BlockManager.spawnBlock(nVIndex, clicked, false);
                spawned.setColliderEnabled(true);
            }
            // lines are inserted
            else if (type == BlockManager.INSERT_LINE)
            {
                ActionManager.callCurrentMode(data);
            }
            // methods or fields are placed
            else if (type == BlockManager.PLACE)
            {
                ActionManager.callAction(ActionManager.NAME_NEW_F_M_V, clicked);
            }


            // delete the clicked block
            else if (modeIndex == ActionManager.DELETE_SELECT)
            {
                ActionManager.callCurrentMode(data);
            }
            // select this block for placing
            else if (type == BlockManager.NAME)
            {
                int variantIndex = BlockManager.getBlockVariantIndex(variant);
                ActionManager.callAction(ActionManager.PLACE_SELECT, variantIndex);
            }
            // try and place a block here
            else if (modeIndex == ActionManager.PLACE_SELECT)
            {
                ActionManager.callCurrentMode(data);
            }
        }
    }



    public class Place : Mode
    {
        // which block variant to place
        private int blockToPlace = -1;

        public override void onCall(object data)
        {
            Block parent = ((Block)data).getParent();

            // place block and enable colliders where necessary
            BlockManager.spawnBlock(blockToPlace, (Block)data);
            parent.setColliderEnabled(true, BlockManager.blocksEnabledForPlacing);
        }

        public override void onSelect(object data)
        {
            multiSelect = true;



            int variantIndex = (int)data;
            blockToPlace = variantIndex;

            // enable only colliders necessary for placement
            WindowManager.updateEditWindowColliders(true, BlockManager.blocksEnabledForPlacing);
        }

        public override void onDeselect()
        {
            // return block collider states to default
            WindowManager.updateEditWindowColliders(true, BlockManager.blocksEnabledDefault);
        }

        public override string getToolsWindowMessage()
        {
            string blockName = BlockManager.getBlockVariant(blockToPlace).getName();
            return ("Placing " + blockName + " block...");
        }
    }



    public class Delete : Mode
    {
        public override void onCall(object data)
        {
            Block toReplace = (Block)data;
            Block master = toReplace.getMasterBlock();
            if (toReplace == master)
            {
                Debug.Log("Hello, ActionManagement here, I'm afraid you cannot delete the master block."); return;
            }

            //TODO: will deleting this affect fields

            BlockManager.spawnBlock(0, toReplace, false);
            // enable only blocks that can be deleted
            master.enableLeafBlocks(true);
        }

        public override void onSelect(object data)
        {
            multiSelect = true;



            // enable only blocks that can be deleted
            WindowManager.enableEditWindowLeafBlocks();
        }

        public override void onDeselect()
        {
            // return block collider states to default
            WindowManager.updateEditWindowColliders(true, BlockManager.blocksEnabledDefault);
        }

        public override string getToolsWindowMessage()
        {
            return ("Deleting...");
        }
    }



    public class InsertLine : Mode
    {
        public override void onCall(object data)
        {
            if (!(data is Block)) return;

            Block block = (Block)data;
            Block toSplit = block.getParent();
            Block splitter = BlockManager.splitBlock(toSplit);

            // if splitting method or field, insert new method/field line
            string blockName = toSplit.getBlockVariant().getName();
            if (blockName == "Field")
                BlockManager.spawnBlock(BlockManager.getBlockVariantIndex("Place Field"), splitter.getSubBlock(1));
            else if (blockName == "VariableH")
                BlockManager.spawnBlock(BlockManager.getBlockVariantIndex(blockName), splitter.getSubBlock(1));

            splitter.setSpecialChildBlock(BlockManager.getBlockVariantIndex("Insert Line"), true);
        }

        public override void onSelect(object data)
        {
            insertLineBlocksEnabled(true);
        }

        public override void onDeselect()
        {
            insertLineBlocksEnabled(false);
        }

        private void insertLineBlocksEnabled(bool enabled)
        {
            if (enabled) WindowManager.updateEditWindowColliders(false);
            else         WindowManager.updateEditWindowColliders(true, BlockManager.blocksEnabledDefault);
            WindowManager.updateEditWindowSpecialBlocks(BlockManager.getBlockVariantIndex("Insert Line"), enabled);
        }

        public override string getToolsWindowMessage()
        {
            return ("Inserting lines...");
        }
    }



    public class CreateName : Mode
    {
        private Window textEntryWindow;
        private NameCreator nameCreator;

        public override void onCall(object data)
        {
            nameCreator.onFinishedNaming(true, (string)data);
            GameObject.Destroy(textEntryWindow.gameObject);
            textEntryWindow = null;

            ActionManager.clearMode();
        }

        public override void onSelect(object data)
        {
            multiSelect = true;



            if (textEntryWindow != null) onDeselect();

            nameCreator = (NameCreator)data;

            textEntryWindow = WindowManager.spawnWindow("TextEntry");
            ((TitledWindow)textEntryWindow).setTitle(nameCreator.getTextEntryWindowMessage());
        }

        public override void onDeselect()
        {
            if (textEntryWindow != null)
            {
                nameCreator.onFinishedNaming(false, null);
                GameObject.Destroy(textEntryWindow.gameObject);
                textEntryWindow = null;
            }
        }

        public override string getToolsWindowMessage()
        {
            return ("Naming...");
        }
    }



    public abstract class NameCreator
    {
        public abstract void onFinishedNaming(bool success, string name);
        public abstract string getTextEntryWindowMessage();
    }



    public class NameNewFMV : NameCreator, Act
    {
        // BlockClicked ==> NameFieldOrMethod ==> CreateName ==> NameFieldOrMethod

        private Block clicked;
        private string blockName;

        public void onCall(object data)
        {
            clicked = (Block)data;
            blockName = clicked.getBlockVariant().getName();

            ActionManager.callAction(ActionManager.CREATE_NAME, this);
        }

        public override void onFinishedNaming(bool success, string name)
        {
            if (!success) return;

            name = ActionManager.stringToSafeName(name);
            if (name.Length == 0) return;


            int emptyNameBlockIndex = 1;
            Block b;
            if (blockName == "Place Variable")
            {
                // spawn a variable block (splittable with insert line)
                b = BlockManager.spawnBlock(BlockManager.getBlockVariantIndex("VariableH"), clicked, false);
            }
            else if (blockName == "Place Field")
            {
                b = BlockManager.spawnBlock(BlockManager.getBlockVariantIndex("Field"), clicked, false);
                clicked.GetComponentInParent<ClassWindow>().referenceTypeSave.addField(b);
            }
            else if (blockName == "Place Method")
            {
                Debug.Log("Not yet implemented.");
                // TODO: call insert method on successful naming
                return;
            }
            else
            {
                Debug.Log("Hello, ActionManagement here, err, trying to name something not nameable."); return;
                return;
            }


            // replace name block
            Block emptyNameBlock = b.getSubBlock(emptyNameBlockIndex);
            int nameBlockVariantIndex = BlockManager.createNameBlock(name);
            BlockManager.spawnBlock(nameBlockVariantIndex, emptyNameBlock);


            b.getMasterBlock().setColliderEnabled(true, BlockManager.blocksEnabledForPlacing);
        }

        public override string getTextEntryWindowMessage()
        {
            return "Name " + blockName.Substring(6) + ':';
        }
    }



    public class NameVariable : NameCreator, Act
    {
        // BlockClicked ==> NameVariable ==> CreateName ==> NameVariable

        private Block beingNamed; // variable declaration block
        private Block beingReplaced; // the empty block

        public void onCall(object data)
        {
            beingNamed = ((Block[])data)[0];
            beingReplaced = ((Block[])data)[1];

            ActionManager.callAction(ActionManager.CREATE_NAME, this);
        }

        public override void onFinishedNaming(bool success, string name)
        {
            if (success)
            {
                // create new block type for this name
                int nameBlockIndex = BlockManager.createNameBlock(name);

                // replace empty block with this new name block
                BlockManager.spawnBlock(nameBlockIndex, beingReplaced, false);

                beingNamed.setColliderEnabled(true, BlockManager.blocksEnabledForPlacing);
            }
            else
            {
                // delete the parent block which has a name as part of it
                BlockManager.spawnBlock(0, beingNamed, false);
            }
        }

        public override string getTextEntryWindowMessage()
        {
            return "Name Variable:";
        }
    }



    /*public class SaveCode : Act
    {
        public void onCall(object data)
        {
            BlockManager.lastFileWindow.saveFile();

            // save custom blocks
            FileManager.saveCustomBlockVariants();
        }
    }



    public class OpenWorkspace : Act
    {
        public void onCall(object data)
        {
            FileManager.loadWorkspace((string)data);

            Window filesWindow = WindowManager.spawnWorkspaceWindow();
            filesWindow.setName((string)data);

            // load custom blocks
            BlockManager.loadCustomBlockVariants();
        }
    }



    public class CreateWorkspace : NameCreator, Act
    {
        public void onCall(object data)
        {
            parent = WindowManager.getWindowWithComponent<WorkspacesButtonManager>();
            ActionManager.callAction(ActionManager.CREATE_NAME, this);
        }

        public override void onFinishedNaming(bool success, string name)
        {
            if (!success) return;

            ActionManager.callAction(ActionManager.OPEN_WORKSPACE, name);
        }

        public override string getTextEntryWindowMessage()
        {
            return "Name Workspace:";
        }
    }



    public class OpenFile : Act
    {
        public void onCall(object data)
        {
            ReferenceTypeS rTS = FileManager.getSourceFile((string)data);
            Window spawned = WindowManager.spawnFileWindow(rTS.isClass);
            ((FileWindow)spawned).referenceTypeSave = rTS;

            // TODO: toggle in list in files window
        }
    }*/



    public class CreateFile : NameCreator, Act
    {
        public void onCall(object data)
        {
            ActionManager.callAction(ActionManager.CREATE_NAME, this);
        }

        public override void onFinishedNaming(bool success, string name)
        {
            if (!success) return;

            ReferenceTypeS rTS = FileManager.createSourceFile(name);
            if (rTS == null) return;

            Window w = WindowManager.spawnWindow("ClassWindow");
            ((ClassWindow)w).referenceTypeSave = rTS;
        }

        public override string getTextEntryWindowMessage()
        {
            return "Name Class:";
        }
    }



    /*public class BackToWorkspaces : Act
    {
        public void onCall(object data)
        {
            WindowManager.spawnWorkspacesWindow();

            // TODO: make user choose whether to save
        }
    }



    public class Compile : Act
    {
        public void onCall(object data)
        {
            CompilationManager.compileActiveWorkspace();
        }
    }*/



    public class CreateSnippet : Act
    {
        public void onCall(object data)
        {
            Window w = WindowManager.spawnWindow("LoneSnippet");
            ((Snippet)w).methodSave = new MethodS();
        }
    }
}
