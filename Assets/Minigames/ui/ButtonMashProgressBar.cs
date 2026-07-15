using System;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class ButtonMashProgressBar : VisualElement
{   
    private float _prog = 0.4f;
    private float _targetOffset = 0.0f;
    private float _targetHeight = 0.0f;

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

    private VisualElement progress;
    private VisualElement target;
    public ButtonMashProgressBar() {
        // -------------------------------------------
        // S E T U P    E L E M E N T S
        // -------------------------------------------
        VisualElement background = new VisualElement { name = "button-mash_background" };
        background.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
        background.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
        // background.style.backgroundColor = new StyleColor(new Color(0.0f, 0.0f, 0.0f, 0.2f));
        background.AddToClassList("button-mash_background");

        progress = new VisualElement { name = "button-mash_progress" };
        progress.style.position = Position.Absolute;
        progress.style.top = 0;
        progress.style.bottom = 0;
        progress.style.left = 0;
        progress.style.right = 0;
        progress.style.scale = new StyleScale(new Vector2(1, progressMeter));
        // progress.style.backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f));
        progress.style.transformOrigin = new TransformOrigin(new Length(50f, LengthUnit.Percent), new Length(100f, LengthUnit.Percent));
        progress.AddToClassList("button-mash_progress");

        target = new VisualElement { name = "button-mash_target" };
        target.style.position = Position.Absolute;
        target.style.top = new StyleLength(new Length(_targetOffset, LengthUnit.Percent));
        target.style.left = 0;
        target.style.right = 0;
        // target.style.backgroundColor = new StyleColor(new Color(0.6f, 0.6f, 0.6f));
        target.AddToClassList("button-mash_target");

        // -------------------------------------------
        // C O N F I G U R E    S T R U C T U R E
        // -------------------------------------------
        Add(background);
        background.Add(target);
        background.Add(progress);
    }

    public bool IsWithinThreshold() {
        // _targetOffset < _prog < (_targetOffset + _targetHeight)
        float p = 100f - (100f*_prog);
        return (p <= (_targetOffset + _targetHeight)) && (p >= _targetOffset);
    }
}
