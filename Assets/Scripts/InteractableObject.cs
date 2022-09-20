using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractableObject : MonoBehaviour {
    protected bool selected = false;
    protected Outline outline;
    protected Color defaultColor;

    // Start is called before the first frame update
    protected virtual void Start() {
        outline = GetComponent<Outline>();
        if (outline == null) {
            outline = gameObject.AddComponent<Outline>();
        }
        defaultColor = outline.OutlineColor;

        Hover(false);
    }

    // Update is called once per frame
    protected abstract void Update();

    /// <summary>
    /// Action of the object when clicked/interacted with.
    /// </summary>
    public abstract void Interact();
    /// <summary>
    /// Actions when object is hovered.
    /// </summary>
    public virtual void Hover(bool isHovering) {
        selected = isHovering;
        outline.OutlineWidth = selected ? 4 : 0; // If the container is not selected, don't highlight it
    }

    public bool isHovered() {
        return selected;
    }
}