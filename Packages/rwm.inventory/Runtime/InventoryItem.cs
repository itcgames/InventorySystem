﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Linq;
using System.IO;

public class InventoryItem : MonoBehaviour
{
    public const string defaultString = "default";
    [Tooltip("This is the name of the item so that the inventory is able to check if it is already in the inventory when adding new items. This name will also be used to display the name " +
        "of the item if the default display is being used and this item is currently selected.")]
    [SerializeField]
    private string _itemTag = null;
    [Tooltip("This is the description of the item that will be displayed if the default display for the inventory is being used and this item is currently selected." +
        "This can be left as null if you are not using the default display inside of the inventory that you will be adding this item to.")]
    [SerializeField]
    [TextArea]
    private string _description = null;
    [Tooltip("Set to true if you want the number of items in the current stack to be displayed inside of the inventory when the item is being hovered over.")]
    [SerializeField]
    private bool _displayNumberOfItems;
    [Tooltip("This is how many of the item is currently in the inventory")]
    private uint _numberOfItems = 0;
    [Tooltip("This is how many of the item you can store per stack")]
    [SerializeField]
    private uint _maxItemsPerStack = 1;
    [Tooltip("Set to true if you want to be able to have more than one of this item per stack." +
        " Updating number of items without setting this to true won't affect the inventory.")]
    [SerializeField]
    private bool _isStackable = true;
    [Tooltip("This is the sprite that will be used to display the item if the default display is being used in the inventory to display. Can be left as null if you do not want to use the" +
        "default display")]
    [SerializeField]
    private Sprite _sprite;
    private Image _image;
    private int _row;
    private int _col;

    public string loadingErrors;
    public string savingErrors;

    [Tooltip("This is the file extension of the sprite that is used to display the item in the default display. The dot before the extension should be included as well.")]
    public string fileTypeOfSprite = ".png";
    public Canvas canvas;
    [SerializeField]
    public delegate bool Use();//should return true if succeeded, otherwise false
    public Use useFunction;
    private Vector3 _position;
    public bool equippableItem;

    [Tooltip("List the names of any of the other scripts attached to the item so that they can be loaded back in. Any data attached to these scripts won't be saved alongside the inventory.")]
    public Type[] scripts;

    public Vector3 Position { get => _position; set => _position = value; }

    public bool DisplayAmountOfItems { get => _displayNumberOfItems; set => _displayNumberOfItems = value; }

    public string Description { get => _description; set => _description = value; }

    public uint NumberOfItems { get => _numberOfItems; set {
            if (value > _maxItemsPerStack)
            {
                _numberOfItems = _maxItemsPerStack;
                return;
            }
            _numberOfItems = value;
            if (!_isStackable)
            {
                _numberOfItems = 1;
                _maxItemsPerStack = 1;
            }
        } }
    public string Name { get => _itemTag; set => _itemTag = value; }
    public uint MaxItemsPerStack { get => _maxItemsPerStack; set {
            if (!_isStackable) _maxItemsPerStack = 1;
            _maxItemsPerStack = value;
        } }
    public bool IsStackable { get => _isStackable; set => _isStackable = value; }

    public Image Image { get => _image; set => _image = value; }

    public Sprite Sprite { get => _sprite; set => _sprite = value; }

    public int Row { get => _row; set => _row = value; }

    public int Col { get => _col; set => _col = value; }

    public void SetToMaxStackAmount()
    {
        _numberOfItems = _maxItemsPerStack;
    }

    private void Start()
    {
        _image = gameObject.GetComponent<Image>();
        if(_image == null)
        {
            _image = gameObject.AddComponent<Image>();
            if(_sprite != null)
            {
                _image.sprite = _sprite;
            }          
        }       
    }

    public void SetUpDisplay()
    {
        RectTransform trans = gameObject.GetComponent<RectTransform>();
        if(trans == null)
        {
            trans = gameObject.AddComponent<RectTransform>();
        }
        trans.localScale = Vector3.one;
        trans.anchoredPosition = (_position != null) ? _position : new Vector3(0,0,0);
        trans.sizeDelta = new Vector2(30, 30);
    }

    public void SetParentTransform(Transform transform)
    {
        if (transform == null || canvas == null) return;
        RectTransform trans = gameObject.GetComponent<RectTransform>();
        trans.SetParent(canvas.transform);
    }

    public void SetCanvasAsParent()
    {
        if (transform == null || canvas == null) return;
        if (transform.parent != canvas.transform)
        {
            RectTransform trans = gameObject.GetComponent<RectTransform>();
            trans.SetParent(canvas.transform);
        }
    }

    public void AddImage()
    {
        _image = gameObject.GetComponent<Image>();
        if (_image == null)
        {
            _image = gameObject.AddComponent<Image>();
            if (_sprite != null)
            {
                _image.sprite = _sprite;
                if(string.IsNullOrEmpty(_image.sprite.name))
                {
                    _image.sprite.name = _sprite.name;
                }               
            }
        }
    }

    public void UseItem()
    {
        _numberOfItems--;
    }

    public ItemData CreateSaveData(bool usingDefaultDisplay)
    {
        savingErrors = "";
        ItemData data = new ItemData();
        data.itemTag = _itemTag;
        data.description = _description;
        data.displayNumberOfItems = _displayNumberOfItems;
        data.numberOfItems = _numberOfItems;
        data.maxItemsPerStack = _maxItemsPerStack;
        data.isStackable = _isStackable;
        data.equippableItem = equippableItem;
        if(usingDefaultDisplay)
        {
            if(gameObject.GetComponent<Image>().sprite != null)
            {
                data.sprite = gameObject.GetComponent<Image>().sprite.name;
            }
            else
            {
                savingErrors += "Sprite Does Not Exist\n";
            }
            if(gameObject.GetComponent<Image>() != null)
            {
                data.image = gameObject.GetComponent<Image>().name;
            }
            else
            {
                savingErrors += "Image Does Not Exist\n";
            }
            if(canvas != null)
            {
                data.canvas = canvas.name;
            }
            else
            {
                savingErrors += "Canvas Does Not Exist.\n";
            }
            data.position = _position;
        }
        data.row = _row;
        data.col = _col;
        data.useFunction = useFunction;
        return data;
    }

    public bool LoadFromData(ItemData data, string spriteLocations)
    {
        loadingErrors = "";
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        equippableItem = data.equippableItem;
        if(canvas == null)
        {
            loadingErrors += "No Canvas Found For Item\n";
        }
        _itemTag = data.itemTag;
        bool useDisplay = data.usingDefaultDisplay;
        if(useDisplay)
        {
            _description = data.description;
            if(string.IsNullOrEmpty(_description))
            {
                loadingErrors += "No description For Item\n";
            }
        }
        try
        {
            //these are all just normal types so they should not be expected to cause an error but could be given weird values if the json has been messed with
            _displayNumberOfItems = data.displayNumberOfItems;
            _numberOfItems = data.numberOfItems;
            _maxItemsPerStack = data.maxItemsPerStack;
            _isStackable = data.isStackable;
        }
        catch (Exception e)
        {
            Debug.LogWarning(e.Message);
            loadingErrors += e.Message + "\n";
            return false;
        }
        if(data.usingDefaultDisplay)
        {
            //there's a possibility that the json file we're using does not have this sprite set or it is set to a different location
            try
            {
                if(string.IsNullOrEmpty(data.sprite))
                {
                    throw new Exception("Sprite Not Set To A Valid Image.");
                }

                byte[] bytes;
                if (string.IsNullOrEmpty(fileTypeOfSprite))
                {
                    bytes = System.IO.File.ReadAllBytes(spriteLocations + data.sprite + ".png");
                    if(!File.Exists(spriteLocations + data.sprite + ".png"))
                    {
                        throw new Exception("File not found");
                    }
                }
                else
                {
                    bytes = System.IO.File.ReadAllBytes(spriteLocations + data.sprite + fileTypeOfSprite);
                    if (!File.Exists(spriteLocations + data.sprite + fileTypeOfSprite))
                    {
                        throw new Exception("File not found");
                    }
                }
                Texture2D texture = new Texture2D(1, 1);
                texture.LoadImage(bytes);
                _sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                _sprite.name = data.sprite;
                SetCanvasAsParent();
                SetUpDisplay();
                AddImage();
            }
            catch(Exception e)
            {
                Debug.LogWarning(e.Message);
                loadingErrors += e.Message + "\n";
                loadingErrors += "This error was thrown when trying to load an image for the item. Please check that the file extension is correct and that the file name is spelled correctly. If using your own folder instead of the def" +
                    "ault location make sure that this folder exists. This may be a problem with how the sprite locations were saved inside of the inventory and not the item itself\n";
                try
                {
                    byte[] bytes;
                    bytes = System.IO.File.ReadAllBytes(spriteLocations + "ErrorSprite.png");
                    Texture2D texture = new Texture2D(1, 1);
                    texture.LoadImage(bytes);
                    _sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    _sprite.name = "Error-Sprite";
                    SetCanvasAsParent();
                    SetUpDisplay();
                    AddImage();
                }
                catch(Exception error)
                {
                    Debug.LogWarning(e.Message);
                    loadingErrors += e.Message + "\n";
                    loadingErrors += "Unable to load error sprite.\n";
                }
                return false;
            }
        }

        gameObject.SetActive(false);
        _row = data.row;
        _col = data.col;
        _position = data.position;
        gameObject.tag = "Item";
        return true;
    }
}

[Serializable]
public class ItemData
{
    public const string defaultString = "default";
    public string itemTag = null;
    public string description = null;
    public bool displayNumberOfItems;
    public uint numberOfItems = 0;
    public uint maxItemsPerStack = 1;
    public bool isStackable = true;
    public bool usingDefaultDisplay = false;
    public string sprite;
    public string image;
    public int row;
    public int col;
    public string canvas;
    public InventoryItem.Use useFunction;
    public Vector3 position;
    public bool equippableItem;
}
