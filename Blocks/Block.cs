using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ActionManagement;

public class Block : MonoBehaviour
{
    private BlockManager.BlockVariant blockVariant;
    public BlockManager.BlockVariant getBlockVariant()
    {
        return blockVariant;
    }
    private List<Block> subBlocks = null; // TODO: make private

    private int width;
    public int getWidth() { return width; }
    private int height;
    public int getHeight() { return height; }

    [SerializeField] private TextMeshPro textBox;





    public void initialise(int blockVariantIndex)
    {
        BlockManager.BlockVariant blockVariant = BlockManager.getBlockVariant(blockVariantIndex);

        string[] subBlockTypes = blockVariant.getSubBlockTypes();

        BlockSave[] subBlockSaves = new BlockSave[subBlockTypes.Length];
        for (int i = 0; i < subBlockTypes.Length; i++)
        {
            int bVI = 0; // empty block by default
            if (BlockManager.isCycleable(subBlockTypes[i]))
                bVI = BlockManager.getFirstVariantOfType(subBlockTypes[i]); // e.g., special AM block
            else if (subBlockTypes[i] == BlockManager.PLACE)
                bVI = BlockManager.getBlockVariantIndex("Place Variable"); // special [+] block
            else if (subBlockTypes[i] == BlockManager.NEW_NAME)
                bVI = BlockManager.getBlockVariantIndex("Custom");

            subBlockSaves[i] = new BlockSave(bVI, null);
        }

        BlockSave blockSave = new BlockSave(blockVariantIndex, subBlockSaves);
        initialise(blockSave);
    }

    public void initialise(BlockSave blockSave)
    {
        this.blockVariant = BlockManager.getBlockVariant(blockSave.blockVariant);
        this.subBlocks = new List<Block>();
        this.gameObject.name = this.blockVariant.getName();
        // set colour
        transform.GetComponentInChildren<MeshRenderer>().material.color = blockVariant.getColor();




        // SPECIAL BLOCKS setup
        string blockType = blockVariant.getBlockType();
        // if insert line, set collider enabled
        if (blockType == BlockManager.INSERT_LINE)
            setBlockButtonActive(true);
        // if name required, call naming action
        if (blockType == BlockManager.NEW_NAME)
            ActionManager.callAction(ActionManager.NAME_VARIABLE, new Block[] { getParent(), this });




        if (blockSave.subBlocks == null) return;
        // spawn all sub blocks
        for (int i = 0; i < blockSave.subBlocks.Length; i++)
        {
            Transform subBlock = Instantiate(BlockManager.blockFab, transform).transform;

            Block subBlockScript = subBlock.GetComponent<Block>();
            subBlockScript.initialise(blockSave.subBlocks[i]);
            subBlocks.Add(subBlockScript);
        }
    }

    public BlockSave saveBlock()
    {
        BlockSave[] blockSaves = new BlockSave[subBlocks.Count];
        for (int i = 0; i < subBlocks.Count; i++)
            blockSaves[i] = subBlocks[i].saveBlock();

        BlockSave bS = new BlockSave(BlockManager.getBlockVariantIndex(blockVariant), blockSaves);
        return bS;
    }





    public void drawBlock(bool master)
    {
        int i = 0;
        foreach (Block subBlock in subBlocks)
        {
            if (subBlock == null)
                Debug.Log(blockVariant.getName() + " null child at pos " + i);
            else
                subBlock.drawBlock(false);
            i++;
        }

        populateTextBox();
        resizeBlock();

        if (master)
        {
            Window w = transform.GetComponentInParent<Window>();
            if (w != null) w.scaleWindow();
        }
    }

    // fills text box with text, updates width and height, and moves subblocks
    private void populateTextBox()
    {
        string text = getBlockText(false);

        textBox.text = text;
    }

    // recursive if wanting text from this block and subblocks together
    public string getBlockText(bool recursive)
    {
        // if editor only block then return empty
        if (recursive && blockVariant.getEditorOnly()) return "";


        // TODO: string builder
        width = -1;
        height = blockVariant.getHeight();
        int extraHeight = 0;

        string[] lines = blockVariant.getLines();
        int[,] subBlockPositions = blockVariant.getSubBlockPositions();

        for (int i = 0; i < subBlockPositions.GetLength(0); i++)
        {
            Block block = subBlocks[i];

            int currentLine = subBlockPositions[i, 0];
            int posInLine = subBlockPositions[i, 1];

            // split line into two strings, one before @ and one after @
            string before = lines[currentLine].Substring(0, posInLine + 1);
            string after = lines[currentLine].Substring(posInLine + 3);

            if (recursive)
            {
                string blockText = block.getBlockText(true);
                lines[currentLine] = before + blockText + after;
            }
            else // create a blank area which subblocks[i] will be on top
            {
                // if this is a multi-line block
                string newLine = before + new string(' ', block.getWidth() - 1) + after;
                if (block.getHeight() > 1)
                {
                    lines[currentLine] = newLine + new string('\n', block.getHeight() - 1);
                }
                // if this is a single-line block
                else
                {
                    lines[currentLine] = newLine;
                }


                // move subblock
                Vector3 sBP = FontManager.lettersAndLinesToVector(posInLine, -(currentLine + extraHeight));
                sBP.z = block.transform.localPosition.z;
                block.transform.localPosition = sBP;


                // update width and height
                if (lines[currentLine].IndexOf('\n') >= 0)
                {
                    // increment height
                    extraHeight += (block.getHeight() - 1);

                    // compute width
                    string[] split = lines[currentLine].Split('\n');
                    foreach (string s in split)
                        if (s.Length > width) width = s.Length;
                }
                else
                {
                    // compute width
                    if (lines[currentLine].Length > width)
                        width = lines[currentLine].Length;
                }
            }



            // if we need to insert another block on this line, recalculate block positions
            if (i + 1 < subBlockPositions.GetLength(0) && subBlockPositions[i + 1, 0] == currentLine)
                subBlockPositions = BlockManager.getSubBlockPositions(lines);
        }
        if (width < 0) width = blockVariant.getWidth();
        height += extraHeight;





        // flatten into a single string
        string text = "";
        for (int i = 0; i < lines.Length; i++)
        {
            // get rid of all @s, we had to keep them for subBlockPositions
            if (recursive)
            {
                // remove @
                int j = lines[i].IndexOf('@');
                while (j >= 0)
                {
                    string before = (j == 0 ? "" : lines[i].Substring(0, j));
                    string after = lines[i].Substring(j + 1);
                    lines[i] = before + after;

                    j = lines[i].IndexOf('@');
                }
            }
            else lines[i] = lines[i].Replace('@', ' '); // replace @ with space

            // add to flattened
            text += lines[i];
            if (i != lines.Length - 1) text += '\n';
        }

        return text;
    }

    // resizes this block
    private void resizeBlock()
    {
        Vector2 planeSize = FontManager.lettersAndLinesToVector(width, height);

        // body plane
        Transform body = transform.GetChild(0);
        body.localScale = new Vector3(planeSize.x, planeSize.y, 1f);

        // body text
        RectTransform tB = (RectTransform)textBox.transform;
        tB.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, planeSize.x);
        tB.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, planeSize.y);
        tB.localPosition = new Vector3(planeSize.x / 2f, -planeSize.y / 2f, tB.localPosition.z);
    }





    public void setColliderEnabled(bool enabled, List<string> mask = null, bool invert = false)
    {
        foreach (Block subBlock in subBlocks)
            subBlock.setColliderEnabled(enabled, mask, invert);

        if (mask == null)
        {
            setBlockButtonActive(enabled);
            return;
        }

        bool contains = mask.Contains(blockVariant.getBlockType());
        if (invert ? !contains : contains)
            setBlockButtonActive(enabled);
        else
            setBlockButtonActive(!enabled);
    }

    private void setBlockButtonActive(bool active)
    {
        transform.GetChild(0).Find("BlockButton").gameObject.SetActive(active);
    }

    public void setSpecialChildBlock(int variantIndex, bool enabled)
    {
        foreach (Block subBlock in subBlocks)
            subBlock.setSpecialChildBlock(variantIndex, enabled);


        Vector3 localPos = new Vector3(0f, 0f, -0.1f);
        if (variantIndex == BlockManager.getBlockVariantIndex("Insert Line"))
        {
            // conditions for insert line
            if (blockVariant.getSplittableV())
                localPos.y -= ((float)height - 0.5f) * FontManager.lineHeight;
            else if (blockVariant.getSplittableH())
                localPos.x += ((float)width - 0.5f) * FontManager.horizontalAdvance;
            else return;
        }
        else return;


        Block special = findSpecialBlock(variantIndex);
        if (enabled)
        {
            // spawn special block
            if (special == null)
            {
                GameObject subBlock = Instantiate(BlockManager.blockFab, transform);
                special = subBlock.GetComponent<Block>();
                special.initialise(variantIndex);
                special.drawBlock(false);
            }
            special.transform.localPosition = localPos;
        }
        else
        {
            // destroy special block
            if (special != null)
                Destroy(special.gameObject);
        }
    }

    private Block findSpecialBlock(int variantToFind)
    {
        foreach (Transform child in transform)
        {
            Block block = child.GetComponent<Block>();
            if (block != null)
            {
                int blockVariant = BlockManager.getBlockVariantIndex(block.getBlockVariant());
                if (blockVariant == variantToFind)
                    return block;
            }
        }
        return null;
    }

    public bool enableLeafBlocks(bool master) // returns isDeleteable
    {
        // TODO: deal with locked files

        bool isDeleteable = blockVariant.getDeleteable();

        foreach (Block subBlock in subBlocks)
            if (subBlock.enableLeafBlocks(false))
                isDeleteable = false;

        // handle special cases
        if (blockVariant.getBlockType() == BlockManager.NAME)
        {
            // NAME blocks may NOT be deleted if
            if (master)
                isDeleteable = false; // is master
            else
            {
                string parentType = getParent().getBlockVariant().getBlockType();
                if (parentType == BlockManager.FIELD) // is the name of a field
                    isDeleteable = false;
            }
        }

        setBlockButtonActive(isDeleteable);
        return isDeleteable;
    }





    public int getSubBlockIndex(Block b)
    {
        return subBlocks.IndexOf(b);
    }

    public Block getSubBlock(int index)
    {
        return subBlocks[index];
    }

    public void replaceSubBlock(Block b, int index, bool delete = true)
    {
        if (delete) Destroy(subBlocks[index].gameObject);
        subBlocks[index] = b;
    }





    public Block getParent()
    {
        return transform.parent.GetComponent<Block>();
    }

    public Block getMasterBlock()
    {
        if (getParent() != null)
        {
            return getParent().getMasterBlock();
        }
        return this;
    }

    public List<Block> getBlocksOfVariant(BlockManager.BlockVariant bV, List<Block> blocks = null)
    {
        if (blocks == null) blocks = new List<Block>();

        if (blockVariant == bV)
            blocks.Add(this);

        foreach (Block subBlock in subBlocks)
            subBlock.getBlocksOfVariant(bV, blocks);

        return blocks;
    }

    public void pressed()
    {
        ActionManager.callAction(ActionManager.BLOCK_CLICKED, GetComponentInParent<Block>());
    }
}
