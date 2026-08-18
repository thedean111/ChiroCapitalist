using System;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class ButtonMashProgressBar : VisualElement
{   
    private float _prog = 0.4f;
    private float _targetOffset = 0.0f;
    private float _targetHeight = 0.0f;
    private Texture2D _inputIcon;

    [UxmlAttribute] [Range(0f,1f)] 
    public float progressMeter {
        get => _prog;
        set {
            _prog = Mathf.Clamp01(value);
            progress.style.scale = new Scale(new Vector3(1, _prog, 1));
        }
    }
    [UxmlAttribute] [Range(0, 100)]
    public float targetOffset {
        get => _targetOffset;
        set {
            _targetOffset = value;
            target.style.top = new StyleLength(new Length(targetOffset, LengthUnit.Percent));
        }
    }
    [UxmlAttribute] [Range(0, 100)]
    public float targetHeight {
        get => _targetHeight;
        set {
            _targetHeight = value;
            target.style.height = new StyleLength(new Length(_targetHeight, LengthUnit.Percent));
        }
    }

    [UxmlAttribute]
    public Texture2D inputIcon {
        get => _inputIcon;
        set {
            inputIconElement.style.backgroundImage = value;
            _inputIcon = value;
        }
    }

    private VisualElement progress;
    private VisualElement target;
    private VisualElement inputIconElement;
    public ButtonMashProgressBar() {
        // -------------------------------------------
        // S E T U P    E L E M E N T S
        // -------------------------------------------
        VisualElement barContainer = new VisualElement { name = "button-mash_bar-container" };
        barContainer.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
        barContainer.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
        barContainer.style.overflow = Overflow.Hidden;
        barContainer.AddToClassList("button-mash_bar-container");

        VisualElement background = new VisualElement { name = "button-mash_background" };
        background.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
        background.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
        background.pickingMode = PickingMode.Ignore;
        background.AddToClassList("button-mash_background");

        progress = new VisualElement { name = "button-mash_progress" };
        progress.style.position = Position.Absolute;
        progress.style.top = 0;
        progress.style.bottom = 0;
        progress.style.left = 0;
        progress.style.right = 0;
        progress.style.scale = new StyleScale(new Vector2(1, progressMeter));
        progress.style.transformOrigin = new TransformOrigin(new Length(50f, LengthUnit.Percent), new Length(100f, LengthUnit.Percent));
        progress.pickingMode = PickingMode.Ignore;
        progress.AddToClassList("button-mash_progress");

        target = new VisualElement { name = "button-mash_target" };
        target.style.position = Position.Absolute;
        target.style.top = new StyleLength(new Length(_targetOffset, LengthUnit.Percent));
        target.style.left = 0;
        target.style.right = 0;
        target.pickingMode = PickingMode.Ignore;
        target.AddToClassList("button-mash_target");

        inputIconElement = new VisualElement { name = "button-mash_input-icon" };
        // inputIconElement.style.width = 100;
        // inputIconElement.style.height = 100;
        inputIconElement.AddToClassList("button-mash_input-icon");

        // -------------------------------------------
        // C O N F I G U R E    S T R U C T U R E
        // -------------------------------------------
        Add(barContainer);
        Add(inputIconElement);
        barContainer.Add(background);
        background.Add(target);
        background.Add(progress);
    }

    public bool IsWithinThreshold() {
        // _targetOffset < _prog < (_targetOffset + _targetHeight)
        float p = 100f - (100f*_prog);
        return (p <= (_targetOffset + _targetHeight)) && (p >= _targetOffset);
    }
}
