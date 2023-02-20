using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using ActionManagement;
using FileManagement;

public class BlockManager : MonoBehaviour
{
    // GENERAL STATE
    public static BlockManager singleton = null;

    [SerializeField] private GameObject _blockFab;
    public static GameObject blockFab { get { return singleton._blockFab; } }

    [SerializeField] private float _blockScale = 1f;
    public static float blockScale { get { return singleton._blockScale; } }

    [SerializeField] private int placeableVariants;

    private static int nextKey;

    //[SerializeField] private bool safeMode = true;





    // EDIT STATE
    //public static FileWindow lastFileWindow;
    private static Block _lastMaster;
    public static Block lastMaster
    {
        get
        {
            return _lastMaster;
        }
        set
        {
            if (value == null) return;

            _lastMaster = value;
            _lastMaster.drawBlock(true);

            /*FileWindow fW = _lastMaster.transform.GetComponentInParent<FileWindow>();
            if (fW != null)
            {
                lastFileWindow = fW;
                if (!fW.simpleView) WindowManager.moveEditToolWindows();
            }*/
        }
    }





    // BLOCK TYPES - needs overhaul, fuzzy line between type and variant
    // special
    public readonly static string EMPTY = "EY";
    public readonly static string ANY = "AY";
    public readonly static string SPLITTER = "SR";
    public readonly static string INSERT_LINE = "IL";
    public readonly static string PLACE = "PL";
    public readonly static string OPEN_METHOD = "OM";

    // struct, class, interface, enum, and record constructs
    public readonly static string FIELD = "FD"; // @AM @TP @NM
    public readonly static string METHOD = "MD"; // @AM @TP @NM(@PL)

    // namespace, class name, method name or variable name
    public readonly static string NAME = "NM";
    public readonly static string NEW_NAME = "NN"; // will create text entry

    // for fields and methods
    public readonly static string ACCESS_MODIFIER = "AM"; // public, private, etc.
    public readonly static string TYPE = "TP"; // int, string, etc.

    // for inside methods and constructors
    public readonly static string BODY = "BY"; // if statements, variable declaration, etc.

    // for if statements, while loops
    public readonly static string BOOLEAN_EXPRESSION = "BE"; // true, i == 1, etc.
    public readonly static string TRUE_FALSE = "TF";





    private static readonly string[] cycleable = {
        ACCESS_MODIFIER, TRUE_FALSE
    };
    public static bool isCycleable(string blockType)
    {
        for (int i = 0; i < cycleable.Length; i++)
            if (blockType == cycleable[i]) return true;
        return false;
    }
    public static int cycleBlockVariantIndex(BlockVariant variant)
    {
        string blockType = variant.getBlockType();

        // try get next ahead in list
        bool found = false;
        foreach (KeyValuePair<int, BlockVariant> kvp in _defaultBlockVariants)
        {
            if (!found && kvp.Value == variant) found = true;
            else if (kvp.Value.getBlockType() == blockType) return kvp.Key;
        }
        foreach (KeyValuePair<int, BlockVariant> kvp in _customBlockVariants)
        {
            if (!found && kvp.Value == variant) found = true;
            else if (kvp.Value.getBlockType() == blockType) return kvp.Key;
        }

        // try get first from beginning of list
        return getFirstVariantOfType(blockType);
    }
    public static int getFirstVariantOfType(string blockType)
    {
        foreach (KeyValuePair<int, BlockVariant> kvp in _defaultBlockVariants)
            if (kvp.Value.getBlockType() == blockType) return kvp.Key;
        foreach (KeyValuePair<int, BlockVariant> kvp in _customBlockVariants)
            if (kvp.Value.getBlockType() == blockType) return kvp.Key;

        Debug.Log("Err, there are no blocks of block type " + blockType + " .");
        return -1;
    }





    // BLOCK VARIANTS
    private static Dictionary<int, BlockVariant> _defaultBlockVariants;
    private static Dictionary<int, BlockVariant> _customBlockVariants;

    [SerializeField] private List<BlockVariant> blockVariants;
    [System.Serializable]
    public class BlockVariant
    {
        // int id = Dictionary key
        [SerializeField] private string name;
        [SerializeField] private string blockType;
        private string[] subBlockTypes;

        [SerializeField] private Color color;
        [SerializeField] private bool splittableV;
        [SerializeField] private bool splittableH;
        [SerializeField] private bool deleteable;
        [SerializeField] private bool editorOnly;

        // an array containing the text for each line in the block
        // '@BT' marks an input field, and is replaced in a Block instance
        [SerializeField] private string[] lines;

        // width and height in letters/lines
        private int width;
        private int height;
        // sub block positions in letters/lines
        // each row = { line, pos } / { y, x }
        private int[,] subBlockPositions;


        public string getName()
        {
            return name;
        }
        public string getBlockType()
        {
            return blockType;
        }
        public string[] getSubBlockTypes()
        {
            string[] sBTs = new string[subBlockTypes.Length];
            for (int i = 0; i < sBTs.Length; i++) sBTs[i] = subBlockTypes[i];
            return sBTs;
        }

        public Color getColor() { return new Color(color.r, color.g, color.b, color.a); }
        public bool getSplittableV() { return splittableV; }
        public bool getSplittableH() { return splittableH; }
        public bool getDeleteable() { return deleteable; }
        public bool getEditorOnly() { return editorOnly; }

        // DEFAULT VALUES GET METHODS
        public string[] getLines()
        {
            string[] ls = new string[lines.Length];
            for (int i = 0; i < ls.Length; i++) ls[i] = lines[i];
            return ls;
        }

        public int getWidth() { return width; }
        public int getHeight() { return height; }
        public int[,] getSubBlockPositions()
        {
            int[,] sBPs = new int[subBlockPositions.GetLength(0), 2];
            for (int i = 0; i < sBPs.GetLength(0); i++)
            {
                sBPs[i, 0] = subBlockPositions[i, 0];
                sBPs[i, 1] = subBlockPositions[i, 1];
            }
            return sBPs;
        }
        public int getSubBlockCount()
        {
            return subBlockPositions.GetLength(0);
        }



        // constructor for name blocks
        public BlockVariant(string name)
        {
            this.name = name;
            blockType = NAME;

            color = singleton.variableColor;
            splittableV = false;
            splittableH = false;
            deleteable = false; // TODO: this needs to vary for dec and ref

            lines = new string[] { name };



            calculateInstanceVariables();
        }



        public BlockVariant(BlockVariantS bVS)
        {
            name = bVS.name;
            blockType = bVS.blockType;

            float[] c = bVS.color;
            color = new Color(c[0], c[1], c[2], c[3]);
            splittableV = bVS.splittableV;
            splittableH = bVS.splittableH;
            deleteable = bVS.deleteable;
            editorOnly = bVS.editorOnly;

            lines = bVS.lines;
        }



        public void calculateInstanceVariables()
        {
            // calculate all the default values for a new block using lines
            width = getMaxLineLength(lines);
            height = lines.Length;

            // calculate subBlockPositions
            subBlockPositions = BlockManager.getSubBlockPositions(lines);

            // get subBlockTypes
            subBlockTypes = new string[subBlockPositions.GetLength(0)];
            for (int i = 0; i < subBlockPositions.GetLength(0); i++)
            {
                string line = lines[subBlockPositions[i, 0]];
                int pos = subBlockPositions[i, 1];
                string newType;
                try
                {
                    newType = "" + line[pos + 1] + line[pos + 2];
                }
                catch
                {
                    Debug.Log("Incorrect block line format.");
                    newType = "AY";
                }
                subBlockTypes[i] = newType;
            }
        }
    }
    public static BlockVariant getBlockVariant(int index)
    {
        if (_defaultBlockVariants.ContainsKey(index))
            return _defaultBlockVariants[index];
        if (_customBlockVariants.ContainsKey(index))
            return _customBlockVariants[index];

        Debug.Log("Err, block variant index " + index + " is invalid.");
        return null;
    }
    public static BlockVariant getBlockVariant(string name)
    {
        KeyValuePair<int, BlockVariant> kvp = getKeyValuePair(name);
        return kvp.Value;
    }
    public static int getBlockVariantIndex(string name)
    {
        KeyValuePair<int, BlockVariant> kvp = getKeyValuePair(name);
        return kvp.Key;
    }
    public static int getBlockVariantIndex(BlockVariant bV)
    {
        foreach (KeyValuePair<int, BlockVariant> kvp in _defaultBlockVariants)
            if (kvp.Value == bV) return kvp.Key;
        foreach (KeyValuePair<int, BlockVariant> kvp in _customBlockVariants)
            if (kvp.Value == bV) return kvp.Key;

        Debug.Log("Err, block variant " + bV.getName() + " is not in a dictionary.");
        return -1;
    }
    private static KeyValuePair<int, BlockVariant> getKeyValuePair(string name)
    {
        foreach (KeyValuePair<int, BlockVariant> kvp in _defaultBlockVariants)
            if (kvp.Value.getName() == name) return kvp;
        foreach (KeyValuePair<int, BlockVariant> kvp in _customBlockVariants)
            if (kvp.Value.getName() == name) return kvp;

        Debug.Log("Block variant " + name + " does not exist.");
        return default(KeyValuePair<int, BlockVariant>);
    }
    public static int getNoOfDefaultBlockVariants()
    {
        return _defaultBlockVariants.Count;
    }





    // called when spawning and deleting (/spawning empty block) block
    public static Block spawnBlock(int blockVariant, Block toReplace, bool emptyOnly = true, BlockSave blockSave = null)
    {
        // can this block be replaced
        bool replacingEmpty = toReplace.getBlockVariant().getBlockType().Equals(EMPTY);
        if (emptyOnly == true && !replacingEmpty)
        {
            Debug.Log("Err, trying to replace a non-empty block");
            return null;
        }



        // get info from block being replaced (usually empty block)
        Block parent = toReplace.getParent();
        int subBlockIndex = parent.getSubBlockIndex(toReplace);



        // configure new block
        Block newBlock = Instantiate(blockFab, parent.transform).GetComponent<Block>();
        newBlock.transform.position = toReplace.transform.position;
        if (blockSave != null) newBlock.initialise(blockSave);
        else                   newBlock.initialise(blockVariant);

        // reconfigure parent
        parent.replaceSubBlock(newBlock, subBlockIndex);

        // draw the blocks
        lastMaster = parent.getMasterBlock();

        return newBlock;
    }

    public static Block splitBlock(Block toSplit, bool originalOnTop = true)
    {
        // get info from toSplit
        Block parent = toSplit.getParent();
        if (parent == null)
        {
            Debug.Log("BlockManager here, you cannot split the master block.");
            return null;
        }
        int subBlockIndex = parent.getSubBlockIndex(toSplit);


        // find out whether this is a vertical or horizontal split
        int splitterVariantIndex;
        BlockVariant toSplitVariant = toSplit.getBlockVariant();
        if (toSplitVariant.getSplittableV())
        {
            splitterVariantIndex = getBlockVariantIndex("SplitterV");
        }
        else if (toSplitVariant.getSplittableH())
        {
            splitterVariantIndex = getBlockVariantIndex("SplitterH");
        }
        else
        {
            Debug.Log("BlockManager here, err, you're trying to split a block not splittable.");
            return null;
        }


        // make splitter
        Block splitter = Instantiate(blockFab, parent.transform).GetComponent<Block>();
        splitter.transform.position = toSplit.transform.position;
        splitter.initialise(splitterVariantIndex);

        // reconfigure parent
        parent.replaceSubBlock(splitter, subBlockIndex, false);

        // move toSplit into splitter
        toSplit.transform.parent = splitter.transform;
        toSplit.transform.localPosition += blockFab.transform.position;
        splitter.replaceSubBlock(toSplit, originalOnTop ? 0 : 1);

        // draw the blocks
        lastMaster = parent.getMasterBlock();

        return splitter;
    }

    public static Block createMasterBlock(int bVI, Transform parent, BlockSave blockSave = null)
    {
        Block masterBlock = Instantiate(blockFab, parent).transform.GetComponent<Block>();
        masterBlock.transform.localScale = new Vector3(blockScale, blockScale, blockScale);

        if (blockSave != null) masterBlock.initialise(blockSave);
        else masterBlock.initialise(bVI);

        masterBlock.drawBlock(true);
        return masterBlock;
    }





    public static string blockSaveToText(BlockSave bS)
    {
        // create temporary block for block text compute, not very elegant
        GameObject g = Instantiate(blockFab);
        Block b = g.GetComponent<Block>();
        b.initialise(bS);

        // compute body text
        string blockText = b.getBlockText(true);

        // destroy temporary block
        Destroy(g);

        return blockText;
    }





    public static int getMaxLineLength(string[] lines)
    {
        int longest = -1;
        foreach (string line in lines)
        {
            if (line.Length > longest)
                longest = line.Length;
        }

        return longest;
    }

    public static int[,] getSubBlockPositions(string[] lines)
    {
        List<int[]> positions = new List<int[]>();
        for (int i = 0; i < lines.Length; i++)
        {
            int currentPos = lines[i].IndexOf('@');
            while (currentPos >= 0)
            {
                positions.Add(new int[] { i, currentPos });
                currentPos = lines[i].IndexOf('@', currentPos + 1);
            }
        }


        // list -> 2D array
        int[,] subBlockPositions = new int[positions.Count, 2];
        for (int i = 0; i < positions.Count; i++)
        {
            subBlockPositions[i, 0] = positions[i][0];
            subBlockPositions[i, 1] = positions[i][1];
        }

        return subBlockPositions;
    }

    private static Block getNonSplitterParent(Block b)
    {
        Block highestSplitter = b;
        for (int i = 0; highestSplitter != null; i++)
        {
            Block newParent = highestSplitter.getParent();
            if (newParent.getBlockVariant().getBlockType() != SPLITTER)
                return highestSplitter;

            highestSplitter = newParent;
        }

        return null;
    }





    [SerializeField] private Color variableColor;
    public static int createNameBlock(string name)
    {
        BlockVariant bV = getBlockVariant(name);
        if (bV != null)
        {
            /*if (_defaultBlockVariants.ContainsValue(bV)) return -1;
            else */return getBlockVariantIndex(bV);
        }

        BlockVariant newVariant = new BlockVariant(name);
        nextKey++;
        _customBlockVariants.Add(nextKey, newVariant);

        return nextKey;
    }

    // load custom block variants (if any)
    // TODO: check this is workspace agnostic
    public static void loadCustomBlockVariants()
    {
        _customBlockVariants = new Dictionary<int, BlockVariant>();



        List<BlockVariantS> bVSs = FileManager.loadCustomBlockVariants();
        nextKey = 100; // assumes <100 default blocks

        if (bVSs != null)
        {
            foreach (BlockVariantS bVS in bVSs)
            {
                BlockVariant bV = new BlockVariant(bVS);
                _customBlockVariants.Add(bVS.key, bV);
                bV.calculateInstanceVariables();
            }

            // get highest key value
            Dictionary<int, BlockVariant>.KeyCollection keys = _customBlockVariants.Keys;
            foreach (int key in keys)
                if (key > nextKey)
                    nextKey = key;
        }
    }

    public static Dictionary<int, BlockVariant> getCustomBlockVariants()
    {
        return _customBlockVariants;
    }





    public static List<string> blocksEnabledDefault = new List<string>() { NAME, PLACE };
    public static List<string> blocksEnabledForPlacing = new List<string>() { EMPTY };

    /*// ACTIONS
    private static int mode = 0;
    private static int toPlace = 0;

    // BLOCK CLICKED
    public void blockClicked(Block clicked)
    {
        try
        {
            switch (mode)
            {
                case PLACE:
                    Block parent = clicked.getParent();

                    // place block
                    BlockManager.spawnBlock(blockToPlace, clicked);
                    // enable colliders where necessary
                    parent.setColliderEnabled(true, blocksEnabledForPlacing);
                    break;

                case DELETE:

                    break;

                case INSERT:

                    break;

                default:

                    break;
            }
        }
        catch
        {

        }
    }

    // CANCEL
    public void cancel()
    {
        mode = 0;
        //WindowManager.updateEditWindowColliders(true, blocksEnabledDefault);
    }

    // PLACE
    private static int PLACE = 1;
    public void place(int toPlace)
    {
        this.toPlace = toPlace;
        mode = PLACE;
        //WindowManager.updateEditWindowColliders(true, blocksEnabledForPlacing);
    }

    // DELETE
    private static int DELETE = 2;
    public void delete()
    {
        mode = DELETE;
        //WindowManager.enableEditWindowLeafBlocks();
    }

    // INSERT
    private static int INSERT = 3;
    public void insert()
    {
        mode = INSERT;
    }*/





    void Awake()
    {
        if (singleton == null) singleton = this;
        else Debug.LogError("Two BlockManager singletons.");



        // create default dictionary, TODO: read it from file in assets
        _defaultBlockVariants = new Dictionary<int, BlockVariant>();
        for (int i = 0; i < blockVariants.Count; i++)
        {
            BlockVariant bV = blockVariants[i];
            _defaultBlockVariants.Add(i, bV);
            // initialise blockVariants
            bV.calculateInstanceVariables();
        }



        blocksEnabledDefault.AddRange(cycleable);
    }
}
