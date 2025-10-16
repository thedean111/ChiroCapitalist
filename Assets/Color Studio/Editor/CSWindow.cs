/* Color Studio by Ramiro Oliva (Kronnect)   /
/  Premium assets for Unity on kronnect.com */


using System.IO;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Globalization;

namespace ColorStudio {

    enum ViewMode {
        ColorWheel = 0,
        Palette = 1,
        Tools = 2,
        Compact = 3,
        Inventory = 4,
        Help = 5
    }

    enum ColorEncoding {
        [InspectorName("RGB (0-1)")]
        RGB01 = 0,
        [InspectorName("RGB (0-255)")]
        RGB255 = 1,
        HSL = 2,
        HEX = 3
    }

    public delegate void CSEvent();

    public partial class CSWindow : EditorWindow {

        static class ShaderParams {
            public static int Aspect = Shader.PropertyToID("_Aspect");
            public static int CursorPos = Shader.PropertyToID("_CursorPos");
            public static int KeyColorsData = Shader.PropertyToID("_KeyColorsData");
            public static int KeyColorsCount = Shader.PropertyToID("_KeyColorsCount");
            public static int Key0Pos = Shader.PropertyToID("_Key0Pos");
            public static int Key0Hue = Shader.PropertyToID("_Key0Hue");
            public static int CenterWidth = Shader.PropertyToID("_CenterWidth");
            public static int Saturation = Shader.PropertyToID("_Saturation");
            public static int MinBrightness = Shader.PropertyToID("_MinBrightness");
            public static int MaxBrightness = Shader.PropertyToID("_MaxBrightness");
            public static int Lightness = Shader.PropertyToID("_Lightness");
            public static int AnimTime = Shader.PropertyToID("_AnimTime");
            public static int Color = Shader.PropertyToID("_Color");
            public static int MainTex = Shader.PropertyToID("_MainTex");

            public const string SKW_COMPLEMENTARY = "CW_COMPLEMENTARY";
            public const string SKW_SPLIT_COMPLEMENTARY = "CW_SPLIT_COMPLEMENTARY";
            public const string SKW_ANALOGOUS = "CW_ANALOGOUS";
            public const string SKW_TETRADIC = "CW_TETRADIC";
            public const string SKW_CUSTOM = "CW_CUSTOM";
        }


        const string PREFS_SETTINGS = "Color Studio Settings";
        const float CENTER_WIDTH = 0.2f;
        const float PI2 = Mathf.PI * 2f;
        const float HALF_PI = Mathf.PI * 0.5f;
        const string PRIMARY_INPUT_CONTROL = "CS_PrimaryInputField";

        public static Color currentPrimaryColor = Color.white;
        public static event CSEvent onColorChange;

        [SerializeField] ViewMode viewMode = ViewMode.ColorWheel;
        [SerializeField] Vector2 scrollPos;
        [SerializeField] Color selectionColor;
        [SerializeField] float selectionLightness;
        [SerializeField] ColorEncoding selectionInputMode;
        [SerializeField] string selectionInput;
        [SerializeField] ColorEncoding primaryInputMode;
        [SerializeField] string primaryInput;
        [SerializeField] Color customColorPicker = Color.white;
        [SerializeField] int selectedCustomColor = -1;
        [SerializeField] Vector2 selectionPos;
        [SerializeField] CSPalette otherPalette;
        [SerializeField] int selectedKey = -1;
        [SerializeField] float clickedAngle;
        [SerializeField] Color selectedObjectColor = Color.white;
        [SerializeField] GameObject selectedObject;
        [SerializeField] Color nearestColor;
        [SerializeField] ColorMatchMode colorMatchMode;
        [SerializeField] bool interpolate;
        [SerializeField] Texture2D nearestTexture;
        [SerializeField] Texture2D referenceTexture;
        [SerializeField] ColorSortingCriteria sortingColorsChoice;
        [SerializeField] CSPalette palette;

        public static CSPalette GetPalette() {
            CSWindow cs = GetWindow<CSWindow>();
            if (cs != null) return cs.palette;
            return null;
        }

        GUIContent[] schemeTexts;
        GUIContent[] viewModeTexts;
        Material cwMat;
        bool mouseDown;
        double startAnimation;
        CSPalette[] projectPalettes;
        Texture2D duplicateIcon, trashIcon, upIcon, downIcon, kronnect;
        Vector4[] cwCustomColors;
        double lastClickTime;
        [SerializeField] bool lightnessDotHighlighted;
        [SerializeField] bool draggingLightnessDot;
        [SerializeField] float lightnessClickedAngle;
        [SerializeField] bool lightnessDotRightSide = true;
        [SerializeField] float lightnessDotAngle;
        int undoGroupId = -1;

        public static CSWindow ShowWindow(int tabIndex = -1) {
            Vector3 size = new Vector3(400, 600);
            Vector3 position = new Vector3(Screen.width / 2 - size.x / 2, Screen.height / 2 - size.y / 2);
            Rect rect = new Rect(position, size);
            CSWindow window = GetWindowWithRect<CSWindow>(rect, false, "Palette Studio", true);
            if (tabIndex >= 0) {
                window.viewMode = (ViewMode)tabIndex;
            }
            window.maxSize = new Vector2(2048, 2048);
            window.minSize = new Vector2(205, 20);
            return window;
        }

        void OnEnable() {
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
            string data = EditorPrefs.GetString(PREFS_SETTINGS, JsonUtility.ToJson(this, false));
            if (!string.IsNullOrEmpty(data)) {
                JsonUtility.FromJsonOverwrite(data, this);
            }

            schemeTexts = new GUIContent[] {
                new GUIContent ("Monochromatic"),
                new GUIContent ("Complementary"),
                new GUIContent ("Gradient"),
                new GUIContent ("Analogous"),
                new GUIContent ("Split\nComplementary"),
                new GUIContent ("Accented\nAnalogous"),
                new GUIContent ("Triadic"),
                new GUIContent ("Tetradic"),
                new GUIContent ("Square"),
                new GUIContent ("Spectrum"),
                new GUIContent ("Custom")
            };
            string iconsPath = "Color Studio/Icons/";
            viewModeTexts = new GUIContent[] {
                new GUIContent (Resources.Load<Texture2D> (iconsPath + "colorWheel"), "Color Wheel"),
                new GUIContent (Resources.Load<Texture2D> (iconsPath + "palette"), "Palette"),
                new GUIContent (Resources.Load<Texture2D> (iconsPath + "brush"), "Tools"),
                new GUIContent (Resources.Load<Texture2D> (iconsPath + "compact"), "Compact Mode"),
                new GUIContent (Resources.Load<Texture2D> (iconsPath + "inventory"), "Project Palettes"),
                new GUIContent (Resources.Load<Texture2D> (iconsPath + "help"), "Help & Support")
            };
            duplicateIcon = Resources.Load<Texture2D>(iconsPath + "duplicate");
            trashIcon = Resources.Load<Texture2D>(iconsPath + "trash");
            upIcon = Resources.Load<Texture2D>(iconsPath + "up");
            downIcon = Resources.Load<Texture2D>(iconsPath + "down");
            kronnect = Resources.Load<Texture2D>(iconsPath + "kronnect");
            titleContent = new GUIContent("Color Studio", Resources.Load<Texture2D>(iconsPath + "icon"));
            if (cwCustomColors == null || cwCustomColors.Length == 0) {
                cwCustomColors = new Vector4[CSPalette.MAX_KEY_COLORS];
            }

            if (palette == null) {
                palette = Resources.Load<CSPalette>("Color Studio/DraftPalette");
                if (palette == null) {
                    palette = CreateInstance<CSPalette>();
                }
                if (otherPalette != null) {
                    LoadPalette(otherPalette);
                }
            }

            UpdateCWMaterial();
            SetColorKeys();
            FindProjectPalettes();
            if (selectionColor.a == 0) {
                currentPrimaryColor = Color.white;
            } else {
                currentPrimaryColor = selectionColor;
            }
        }

        void OnDisable() {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            if (palette) {
                EditorUtility.SetDirty(palette);
            }
            // autosave
            if (otherPalette != null) {
                SavePalette();
            }
            string data = JsonUtility.ToJson(this, false);
            EditorPrefs.SetString(PREFS_SETTINGS, data);
        }

        void OnUndoRedoPerformed() {
            if (palette == null) return;
            // Rebuild visuals/materials to reflect undone/redone state
            SetColorKeys();
            UpdateCWMaterial();
            Repaint();
        }

        private void OnDestroy() {
            NewPalette();
            EditorPrefs.SetString(PREFS_SETTINGS, "");
        }

        void OnGUI() {

            bool issueRepaint = false;
            bool paletteChanges = false;

            if (cwMat == null || palette.material == null || palette.material.GetColorArray("_Colors") == null) {
                palette.UpdateMaterial();
                UpdateCWMaterial();
                paletteChanges = true;
            }
            EditorGUIUtility.labelWidth = 80;

            EditorGUI.BeginChangeCheck();
            viewMode = (ViewMode)GUILayout.SelectionGrid((int)viewMode, viewModeTexts, 6, GUILayout.MaxHeight(28));
            if (EditorGUI.EndChangeCheck()) {
                if (viewMode == ViewMode.Compact) {
                    maxSize = new Vector2(maxSize.x, 105);
                    minSize = new Vector2(minSize.x, 105);
                } else {
                    maxSize = new Vector2(4000, 4000);
                    minSize = new Vector2(minSize.x, 200);
                }
                if (viewMode == ViewMode.Inventory) {
                    FindProjectPalettes();
                }
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            Rect space = new Rect();
            Vector3 pos;
            float maxWidth = EditorGUIUtility.currentViewWidth - 12;
            if (Screen.height < 772) maxWidth -= 8;

            EditorGUILayout.Separator();

            Event e = Event.current;

            if (viewMode == ViewMode.ColorWheel) {

                GUILayout.Label("C O L O R   W H E E L");

                // Color sheme selector
                EditorGUI.BeginChangeCheck();
                ColorScheme prevScheme = palette.scheme;
                int xCount = Mathf.Clamp((int)(maxWidth / 95), 2, schemeTexts.Length);
                palette.scheme = (ColorScheme)GUILayout.SelectionGrid((int)palette.scheme, schemeTexts, xCount, GUILayout.MaxWidth(maxWidth));
                if (EditorGUI.EndChangeCheck()) {
                    Undo.RecordObject(palette, "Change Scheme");
                    if (prevScheme == palette.scheme) {
                        palette.SetPrimaryAngle(Random.Range(0, Mathf.PI * 2f));
                    } else {
                        palette.splitAmount = 0.6f;
                        palette.hueCount = palette.scheme.recommendedHues();
                        if (palette.scheme == ColorScheme.Spectrum) {
                            palette.saturation = 1f;
                        }
                    }
                    UpdateCWMaterial();
                    paletteChanges = true;
                    EditorUtility.SetDirty(palette);
                }

                // Draw color wheel
                if (palette.scheme != ColorScheme.Spectrum) {
                    EditorGUILayout.Separator();

                    space = EditorGUILayout.BeginVertical();
                    GUILayout.Space(EditorGUIUtility.currentViewWidth);
                    EditorGUILayout.EndVertical();
                    space.xMin += 10;
                    space.xMax -= 10;
                    space.yMin += 10;
                    space.yMax -= 10;

                    GUI.BeginGroup(space);
                    pos = e.mousePosition;
                    GUI.EndGroup();

                    pos.x /= space.width;
                    pos.y /= space.height;
                    pos.y = 1f - pos.y;
                    Vector2 cursorPos = pos;
                    pos.x -= 0.5f;
                    pos.y -= 0.5f;
                    float d = pos.x * pos.x + pos.y * pos.y;

                    if (d < 0.25f) {
                        float cursorAngle = Mathf.Atan2(pos.y, pos.x) + Mathf.PI;
                        int cursorKey = -1;
                        // If clicking over the color wheel, blur primary input if focused
                        if (e.type == EventType.MouseDown) {
                            if (GUI.GetNameOfFocusedControl() == PRIMARY_INPUT_CONTROL) {
                                GUI.FocusControl(null);
                                GUIUtility.keyboardControl = 0;
                            }
                        }
                        // Compute lightness dot position and hover state
                        // Evaluate lightness dot hover on the closest side (left/right)
                        Vector2 lightnessDotPosRight = GetCWLightnessDotPos(palette.primaryLightness, true);
                        Vector2 lightnessDotPosLeft = GetCWLightnessDotPos(palette.primaryLightness, false);
                        float distR = (cursorPos - lightnessDotPosRight).sqrMagnitude;
                        float distL = (cursorPos - lightnessDotPosLeft).sqrMagnitude;
                        bool hoverLightnessDot = Mathf.Min(distR, distL) < 0.0008f;
                        if (hoverLightnessDot) {
                            lightnessDotRightSide = distR <= distL;
                        }
                        if (!mouseDown) {
                            bool changes = false;
                            for (int k = 0; k < palette.keyColors.Length; k++) {
                                if (palette.keyColors[k].visible) {
                                    float distance = (cursorPos - palette.keyColors[k].pos).sqrMagnitude;
                                    if (distance < 0.0008f) {
                                        if (!palette.keyColors[k].highlighted) {
                                            changes = true;
                                        }
                                        palette.keyColors[k].highlighted = true;
                                        cursorKey = k;
                                    } else {
                                        if (palette.keyColors[k].highlighted) {
                                            changes = true;
                                        }
                                        palette.keyColors[k].highlighted = false;
                                    }
                                }
                            }
                            // highlight lightness dot
                            if (lightnessDotHighlighted != hoverLightnessDot) {
                                lightnessDotHighlighted = hoverLightnessDot;
                                changes = true;
                            }
                            if (changes) {
                                UpdateCWMaterial();
                            }
                        }
                        if (cursorKey < 0 && selectedKey < 0 && e.type == EventType.MouseUp && e.control) {
                            // Add key
                            Undo.RecordObject(palette, "Add Custom Color");
                            AddCustomColor(cursorAngle);
                            paletteChanges = true;
                            EditorUtility.SetDirty(palette);
                        }
                        if (e.type == EventType.MouseDown && !e.control) {
                            double now = EditorApplication.timeSinceStartup;
                            if (cursorKey >= CSPalette.START_INDEX_CUSTOM_COLOR && now - lastClickTime < 0.3f) {
                                Undo.RecordObject(palette, "Delete Custom Color");
                                palette.keyColors[cursorKey].visible = false;
                                cursorKey = -1;
                                paletteChanges = true;
                                EditorUtility.SetDirty(palette);
                            }
                            mouseDown = true;
                            if (hoverLightnessDot && cursorKey < 0) {
                                if (undoGroupId == -1) {
                                    Undo.IncrementCurrentGroup();
                                    Undo.SetCurrentGroupName("Change Lightness");
                                    undoGroupId = Undo.GetCurrentGroup();
                                    Undo.RecordObject(palette, "Change Lightness");
                                }
                                draggingLightnessDot = true;
                                selectedKey = -1;
                                lightnessClickedAngle = cursorAngle;
                                // initialize dragged dot angle to current cursor angle
                                lightnessDotAngle = Mathf.Atan2(cursorPos.y - 0.5f, cursorPos.x - 0.5f);
                            } else {
                                if (undoGroupId == -1) {
                                    Undo.IncrementCurrentGroup();
                                    Undo.SetCurrentGroupName("Move Key");
                                    undoGroupId = Undo.GetCurrentGroup();
                                    Undo.RecordObject(palette, "Move Key");
                                }
                                selectedKey = cursorKey;
                                clickedAngle = cursorAngle;
                                draggingLightnessDot = false;
                            }
                            lastClickTime = now;
                        }
                        if (mouseDown && selectedKey >= 0) {
                            float delta = cursorAngle - clickedAngle;
                            if (delta < -Mathf.PI) {
                                delta = Mathf.PI * 2 + delta;
                            } else if (delta > Mathf.PI) {
                                delta = Mathf.PI * 2 - delta;
                            }
                            bool changed = false;
                            switch (palette.keyColors[selectedKey].type) {
                                case KeyColorType.Primary: {
                                        if (delta != 0) {
                                            palette.SetPrimaryAngle(palette.primaryAngle + delta);
                                            changed = true;
                                        }
                                        // Update saturation from radial position if within middle ring
                                        float r = Mathf.Sqrt(d);
                                        float light = (r - CENTER_WIDTH) / (1f - CENTER_WIDTH);
                                        if (light >= 0f && light <= 0.32f) {
                                            float sat = Mathf.Clamp01(light / 0.32f);
                                            if (Mathf.Abs(sat - palette.primarySaturation) > 0.0001f) {
                                                palette.saturation = sat;
                                                changed = true;
                                            }
                                        }
                                    }
                                    break;
                                case KeyColorType.Complementary:
                                    if (delta != 0) {
                                        switch (palette.scheme.keyAdjustment(selectedKey)) {
                                            case KeyAdjustment.RotateComplementary:
                                                palette.splitAmount += delta;
                                                break;
                                            case KeyAdjustment.RotateComplementaryInverted:
                                                palette.splitAmount -= delta;
                                                break;
                                            case KeyAdjustment.RotatePrimary:
                                                palette.SetPrimaryAngle(palette.primaryAngle + delta);
                                                break;
                                        }
                                        if (palette.scheme != ColorScheme.Gradient) {
                                            palette.splitAmount = Mathf.Clamp(palette.splitAmount, 0, Mathf.PI * 0.5f);
                                        }
                                        changed = true;
                                    }
                                    break;
                                case KeyColorType.Custom:
                                    if (delta != 0) {
                                        float newAngle = palette.keyColors[selectedKey].angle + delta;
                                        palette.SetKeyColor(selectedKey, KeyColorType.Custom, newAngle);
                                        UpdateKeyVisuals(selectedKey, newAngle, KeyColorType.Custom);
                                        changed = true;
                                    }
                                    break;
                            }
                            if (changed) {
                                clickedAngle = cursorAngle;
                                paletteChanges = true;
                                UpdatePrimaryInput();
                            }
                        }
                        
                    }

                    // Allow lightness dot to keep dragging even if pointer is outside the color wheel
                    if (mouseDown && draggingLightnessDot) {
                        Vector2 projectedPos = ProjectCursorToOuterRing(cursorPos);
                        float newLightness = Mathf.Clamp01(1f - projectedPos.y);
                        if (Mathf.Abs(newLightness - palette.primaryLightness) > 0.0001f) {
                            if (undoGroupId == -1) {
                                Undo.IncrementCurrentGroup();
                                Undo.SetCurrentGroupName("Change Lightness");
                                undoGroupId = Undo.GetCurrentGroup();
                                Undo.RecordObject(palette, "Change Lightness");
                            }
                            palette.lightness = newLightness;
                            paletteChanges = true;
                            UpdatePrimaryInput();
                            EditorUtility.SetDirty(palette);
                        }
                        // Update side dynamically to allow full circular dragging
                        lightnessDotRightSide = projectedPos.x >= 0.5f;
                        // Track current angle so the dot follows the pointer
                        lightnessDotAngle = Mathf.Atan2(projectedPos.y - 0.5f, projectedPos.x - 0.5f);
                        // Keep it highlighted while dragging
                        if (!lightnessDotHighlighted) {
                            lightnessDotHighlighted = true;
                        }
                        UpdateCWMaterial();
                    }

                    EditorGUI.DrawPreviewTexture(space, Texture2D.whiteTexture, cwMat, ScaleMode.ScaleToFit);

                    // Draw tooltip with RGB/HSL when hovering any color dot (including lightness dot)
                    {
                        // Compute proximity to key dots and lightness dot directly
                        int hoveredKey = -1;
                        float bestDist = float.MaxValue;
                        for (int k = 0; k < palette.keyColors.Length; k++) {
                            if (!palette.keyColors[k].visible) continue;
                            float dist = (cursorPos - palette.keyColors[k].pos).sqrMagnitude;
                            if (dist < bestDist) { bestDist = dist; hoveredKey = k; }
                        }
                        Vector2 lPosNow = GetCWLightnessDotPos(palette.primaryLightness, lightnessDotRightSide);
                        float lightDist = (cursorPos - lPosNow).sqrMagnitude;
                        const float showThreshold = 0.0016f; // slightly larger than highlight radius
                        bool hoveredLightness = lightDist < bestDist && lightDist < showThreshold;
                        bool showKey = hoveredKey >= 0 && bestDist < showThreshold;
                        if (showKey || hoveredLightness) {
                            Vector2 uvPos;
                            float hue, saturation, lightness;
                            if (showKey) {
                                uvPos = palette.keyColors[hoveredKey].pos;
                                hue = palette.keyColors[hoveredKey].hue;
                                saturation = palette.saturation;
                                lightness = palette.lightness;
                            } else {
                                uvPos = lPosNow;
                                hue = ((palette.primaryAngle % PI2 + PI2) % PI2) / PI2;
                                saturation = palette.primarySaturation;
                                lightness = palette.primaryLightness;
                            }
                            Color tooltipColor = ColorConversion.GetColorFromHSL(hue, saturation, lightness);
                            Vector2 screenPos = new Vector2(space.xMin + uvPos.x * space.width, space.yMax - uvPos.y * space.height);
                            string tip;
                            if (!showKey && hoveredLightness) {
                                tip = "Lightness " + lightness.ToString("F3", CultureInfo.InvariantCulture);
                            } else {
                                int r255 = Mathf.RoundToInt(tooltipColor.r * 255f);
                                int g255 = Mathf.RoundToInt(tooltipColor.g * 255f);
                                int b255 = Mathf.RoundToInt(tooltipColor.b * 255f);
                                tip = "RGB " + r255.ToString(CultureInfo.InvariantCulture) + ", " + g255.ToString(CultureInfo.InvariantCulture) + ", " + b255.ToString(CultureInfo.InvariantCulture)
                                    + "\nHSL " + hue.ToString("F3", CultureInfo.InvariantCulture) + ", " + saturation.ToString("F3", CultureInfo.InvariantCulture) + ", " + lightness.ToString("F3", CultureInfo.InvariantCulture);
                            }

                            // Layout and background
                            GUIStyle textStyle = new GUIStyle(EditorStyles.label);
                            textStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                            string[] lines = tip.Split('\n');
                            float maxW = 0f;
                            for (int i = 0; i < lines.Length; i++) {
                                Vector2 sz = textStyle.CalcSize(new GUIContent(lines[i]));
                                if (sz.x > maxW) maxW = sz.x;
                            }
                            float lineH = EditorGUIUtility.singleLineHeight;
                            float padX = 10f, padY = 8f;
                            float width = maxW + padX + 8f; // extra space to avoid text clipping
                            float height = lines.Length * lineH + padY;
                            Rect tipRect = new Rect(screenPos.x + 12, screenPos.y - height - 8, width, height);
                            // Clamp to the color wheel rect to avoid clipping by scroll view edges
                            Rect clip = space;
                            if (tipRect.xMax > clip.xMax - 4) tipRect.x = clip.xMax - tipRect.width - 4;
                            if (tipRect.yMin < clip.yMin + 4) tipRect.y = screenPos.y + 12;
                            if (tipRect.xMin < clip.xMin + 4) tipRect.x = clip.xMin + 4;
                            if (tipRect.yMax > clip.yMax - 4) tipRect.y = clip.yMax - tipRect.height - 4;

                            Color bg = EditorGUIUtility.isProSkin ? new Color(0, 0, 0, 0.9f) : new Color(1, 1, 1, 0.95f);
                            EditorGUI.DrawRect(tipRect, bg);
                            Rect textRect = new Rect(tipRect.x + 6, tipRect.y + 4, tipRect.width - 12, tipRect.height - 8);
                            GUI.Label(textRect, tip, textStyle);
                        }
                    }

                    if (palette.customColorsCount > 0) {
                        space.x = space.xMax - 50;
                        space.height = 20;
                        space.width = 50;
                        if (GUI.Button(space, "Clear", EditorStyles.miniButton)) {
                            Undo.RecordObject(palette, "Clear Custom Colors");
                            ClearCustomColors();
                            paletteChanges = true;
                            EditorUtility.SetDirty(palette);
                        }
                    }
                }
            }

            if (viewMode == ViewMode.Palette || viewMode == ViewMode.ColorWheel) {
                GUILayout.Label("P R I M A R Y");
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                primaryInputMode = (ColorEncoding)EditorGUILayout.EnumPopup(primaryInputMode, GUILayout.MaxWidth(60));
                if (EditorGUI.EndChangeCheck()) {
                    UpdatePrimaryInput();
                }

                GUI.SetNextControlName(PRIMARY_INPUT_CONTROL);
                primaryInput = EditorGUILayout.TextField(GUIContent.none, primaryInput, GUILayout.MaxWidth(maxWidth - 110));
                GUI.enabled = !string.IsNullOrEmpty(primaryInput);
                if (GUILayout.Button("Select", EditorStyles.miniButtonRight, GUILayout.MaxWidth(50))) {
                    primaryInput = primaryInput.Trim();
                    if (SelectPrimaryInput()) {
                        paletteChanges = true;
                    }
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
 
                EditorGUILayout.Separator();

                GUILayout.Label("C U R R E N T   P A L E T T E");

                // Palette modifiers with Undo support
                float newSaturation = EditorGUILayout.Slider("Saturation", palette.saturation, 0, 1);
                float newLightness = EditorGUILayout.Slider("Lightness", palette.lightness, 0, 1);
                int newHueCount = palette.hueCount;
                if (palette.scheme != ColorScheme.Monochromatic && palette.scheme != ColorScheme.Custom) {
                    newHueCount = EditorGUILayout.IntSlider("Hues", palette.hueCount, palette.scheme.minHues(), 128);
                }
                int newShades = EditorGUILayout.IntSlider("Shades", palette.shades, 1, 256);
                float minb = palette.minBrightness, maxb = palette.maxBrightness;
                EditorGUILayout.MinMaxSlider("Brightness", ref minb, ref maxb, 0, 1);
                int newKelvin;
                float newCTS;
                EditorGUILayout.BeginHorizontal();
                newKelvin = EditorGUILayout.IntField(new GUIContent("Kelvin", "Color temperature (1000-40.000 ºKelvin)"), palette.kelvin);
                newKelvin = Mathf.Clamp(newKelvin, 1000, 40000);
                newCTS = EditorGUILayout.Slider(palette.colorTempStrength, 0, 1f);
                EditorGUILayout.EndHorizontal();

                bool anyChange =
                    !Mathf.Approximately(newSaturation, palette.saturation) ||
                    !Mathf.Approximately(newLightness, palette.lightness) ||
                    newHueCount != palette.hueCount ||
                    newShades != palette.shades ||
                    !Mathf.Approximately(minb, palette.minBrightness) ||
                    !Mathf.Approximately(maxb, palette.maxBrightness) ||
                    newKelvin != palette.kelvin ||
                    !Mathf.Approximately(newCTS, palette.colorTempStrength);

                if (anyChange) {
                    Undo.RecordObject(palette, "Modify Palette");
                    palette.saturation = newSaturation;
                    palette.lightness = newLightness;
                    palette.hueCount = newHueCount;
                    palette.shades = newShades;
                    palette.minBrightness = minb;
                    palette.maxBrightness = maxb;
                    palette.kelvin = newKelvin;
                    palette.colorTempStrength = newCTS;
                    UpdateCWMaterial();
                    paletteChanges = true;
                    EditorUtility.SetDirty(palette);
                }
            }

            if (viewMode == ViewMode.Tools) {
                DrawConversionTools();
            }

            // Draw palette

            if (viewMode == ViewMode.ColorWheel || viewMode == ViewMode.Palette || viewMode == ViewMode.Tools) {
                EditorGUILayout.BeginVertical(GUI.skin.box);
            }

            if (viewMode != ViewMode.Inventory && viewMode != ViewMode.Help) {
                space = EditorGUILayout.BeginVertical();
                float paletteRowSize;
                if (viewMode == ViewMode.Palette) {
                    paletteRowSize = Mathf.Max(64, EditorGUIUtility.currentViewWidth);
                } else {
                    paletteRowSize = 64;
                }
                GUILayout.Space(paletteRowSize);
                EditorGUILayout.EndVertical();
            }

            if (viewMode == ViewMode.ColorWheel || viewMode == ViewMode.Palette || viewMode == ViewMode.Tools) {
                GUI.BeginGroup(space);
                Vector3 palettePos = e.mousePosition;
                palettePos.x /= space.width;
                palettePos.y /= space.height;
                palettePos.y = 1f - palettePos.y;
                GUI.EndGroup();
                if (space.height != 0) {
                    palette.material.SetFloat(ShaderParams.Aspect, (float)space.width / space.height);
                }

                // Ensure palette colors/material reflect any changes (e.g., lightness drag) before drawing
                if (paletteChanges) {
                    SetColorKeys();
                    paletteChanges = false;
                }

                EditorGUI.DrawPreviewTexture(space, Texture2D.whiteTexture, palette.material);

                if (viewMode != ViewMode.Tools) {
                    palette.material.SetVector(ShaderParams.CursorPos, selectionPos);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUI.BeginChangeCheck();
                    selectionInputMode = (ColorEncoding)EditorGUILayout.EnumPopup(selectionInputMode, GUILayout.Width(60));
                    if (EditorGUI.EndChangeCheck()) {
                        UpdateSelectionInput();
                    }

                    selectionInput = EditorGUILayout.TextField(GUIContent.none, selectionInput, GUILayout.MaxWidth(maxWidth - 116));
                    GUI.enabled = !string.IsNullOrEmpty(selectionInput);
                        if (GUILayout.Button("Add", EditorStyles.miniButtonRight, GUILayout.MaxWidth(50))) {
                            Undo.RecordObject(palette, "Add Custom Color");
                            AddSelectionInput();
                    }
                    GUI.enabled = true;
                    EditorGUILayout.EndHorizontal();

                    if (selectedCustomColor >= 0) {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Delete Color", EditorStyles.miniButton)) {
                            Undo.RecordObject(palette, "Delete Custom Color");
                            DeleteColor();
                            EditorUtility.SetDirty(palette);
                        }
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();
                    }
                }
                if (viewMode == ViewMode.Palette) {

                    EditorGUILayout.BeginHorizontal();
                    customColorPicker = EditorGUILayout.ColorField("Color Picker", customColorPicker);
                    if (GUILayout.Button("Add", EditorStyles.miniButtonRight, GUILayout.MaxWidth(50))) {
                        Undo.RecordObject(palette, "Add Custom Color");
                        AddCustomColor(customColorPicker);
                        paletteChanges = true;
                        EditorUtility.SetDirty(palette);
                    }
                    EditorGUILayout.EndHorizontal();
                    CSPalette prevPalette = otherPalette;
                    otherPalette = (CSPalette)EditorGUILayout.ObjectField("Palette", otherPalette, typeof(CSPalette), false);
                        if (otherPalette != prevPalette && otherPalette != null) {
                        LoadPalette(otherPalette);
                        paletteChanges = true;
                    }
                    if (palette.customColorsCount > 1) {
                        EditorGUILayout.BeginHorizontal();
                        sortingColorsChoice = (ColorSortingCriteria)EditorGUILayout.EnumPopup("Custom Colors", sortingColorsChoice);
                        if (GUILayout.Button("Sort", GUILayout.Width(50))) {
                            Undo.RecordObject(palette, "Sort Custom Colors");
                            palette.SortCustomColors(sortingColorsChoice);
                            paletteChanges = true;
                            EditorUtility.SetDirty(palette);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                if (palettePos.x >= 0 && palettePos.x <= 1f && palettePos.y >= 0 && palettePos.y <= 1f) {
                    bool click = e.type == EventType.MouseDown && e.button == 0;
                    if (click) {
                        issueRepaint = true;
                        selectedCustomColor = -1;
                        selectionPos = palettePos;
                        int colorIndex = Mathf.FloorToInt(palettePos.x * palette.colorsCount);
                        int totalRows = Mathf.Max(palette.shades, 1);
                        int row = Mathf.FloorToInt(palettePos.y * totalRows);
                        if (row >= totalRows) row = totalRows - 1;
                        
                        // Determine if this column corresponds to a custom color
                        if (palette.scheme == ColorScheme.Custom) {
                            selectedCustomColor = colorIndex + CSPalette.START_INDEX_CUSTOM_COLOR;
                        } else if (colorIndex >= palette.hueCount) {
                            selectedCustomColor = colorIndex + CSPalette.START_INDEX_CUSTOM_COLOR - palette.hueCount;
                        }
                        
                        int darkRows = Mathf.CeilToInt((totalRows - 1) / 2f);
                        int centerRow = darkRows; // 0..totalRows-1
                        bool isOriginalRow = row == centerRow; // center row equals key color
                        if (selectedCustomColor >= 0) {
                            selectionColor = palette.keyColors[selectedCustomColor].color;
                            selectionColor.ApplyTemperature(palette.kelvin, palette.colorTempStrength);
                        } else {
                            selectionColor = palette.colors[colorIndex];
                        }
                        
                        if (!isOriginalRow && palette.shades > 1) {
                            float t;
                            if (row < centerRow) {
                                int k = centerRow - row; // 1..darkRows
                                float tNorm = (k - 0.5f) / Mathf.Max(darkRows, 1);
                                t = Mathf.Lerp(palette.lightness, palette.minBrightness, tNorm);
                            } else {
                                int lightRows = Mathf.Max(totalRows - 1 - darkRows, 0);
                                int k = row - centerRow; // 1..lightRows
                                float tNorm = (k - 0.5f) / Mathf.Max(lightRows, 1);
                                t = Mathf.Lerp(palette.lightness, palette.maxBrightness, tNorm);
                            }
                            selectionLightness = Mathf.Clamp01(t);
                            selectionColor = ColorConversion.GetColorFromRGBSL(selectionColor.r, selectionColor.g, selectionColor.b, palette.saturation, selectionLightness);
                        } else {
                            // For originals, reflect actual lightness
                            HSLColor hsl = ColorConversion.GetHSLFromRGB(selectionColor);
                            selectionLightness = hsl.l;
                        }

                        currentPrimaryColor = selectionColor;
                        if (onColorChange != null) onColorChange();

                        UpdateSelectionInput();
                    }
                }
                EditorGUILayout.EndVertical();

            } else if (viewMode == ViewMode.Compact) {
                palette.material.SetVector(ShaderParams.CursorPos, Vector3.left);
                EditorGUI.DrawPreviewTexture(space, Texture2D.whiteTexture, palette.material);
            } else if (viewMode == ViewMode.Inventory) {
                palette.material.SetVector(ShaderParams.CursorPos, Vector3.left);
                if (projectPalettes == null) {
                    FindProjectPalettes();
                }

                GUILayout.Label("A L L   P A L E T T E S (" + projectPalettes.Length + ")");

                for (int k = 0; k < projectPalettes.Length; k++) {
                    CSPalette pal = projectPalettes[k];
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    if (pal == null) {
                        GUI.enabled = false;
                        GUILayout.Button("(Deleted)");
                        EditorGUILayout.BeginVertical();
                        GUILayout.Space(64);
                        EditorGUILayout.EndVertical();
                        GUI.enabled = true;

                    } else {
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button(new GUIContent(pal.name, "Click to load " + pal.name))) {
                            EditorGUIUtility.PingObject(pal);
                            LoadPalette(pal);
                            viewMode = ViewMode.Palette;
                            GUIUtility.ExitGUI();
                        }
                        if (GUILayout.Button(new GUIContent("?", "Locate"), GUILayout.MaxWidth(24))) {
                            EditorGUIUtility.PingObject(pal);
                            Selection.activeObject = pal;
                        }
                        GUI.enabled = k > 0;
                        if (GUILayout.Button(new GUIContent(upIcon, "Move Up"), GUILayout.MaxWidth(24), GUILayout.MaxHeight(18))) {
                            pal.order = projectPalettes[k - 1].order - 1;
                            EditorUtility.SetDirty(pal);
                            ResortProjectPalettes();
                        }
                        GUI.enabled = k < projectPalettes.Length - 1;
                        if (GUILayout.Button(new GUIContent(downIcon, "Move Down"), GUILayout.MaxWidth(24), GUILayout.MaxHeight(18))) {
                            pal.order = projectPalettes[k + 1].order + 1;
                            EditorUtility.SetDirty(pal);
                            ResortProjectPalettes();
                        }
                        GUI.enabled = true;
                        if (GUILayout.Button(new GUIContent(duplicateIcon, "Duplicate"), GUILayout.MaxWidth(24), GUILayout.MaxHeight(18))) {
                            if (DuplicatePalette(pal)) {
                                FindProjectPalettes();
                                GUIUtility.ExitGUI();
                            }
                        }
                        if (GUILayout.Button(new GUIContent(trashIcon, "Delete"), GUILayout.MaxWidth(24), GUILayout.MaxHeight(18))) {
                            if (DeletePalette(pal)) {
                                projectPalettes[k] = null;
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                        space = EditorGUILayout.BeginVertical();
                        GUILayout.Space(64);
                        EditorGUILayout.EndVertical();
                        pal.UpdateMaterial();
                        EditorGUI.DrawPreviewTexture(space, Texture2D.whiteTexture, pal.material);
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Separator();
                }
            }

            if (viewMode == ViewMode.Palette) {

                float width = Mathf.Max(20, EditorGUIUtility.currentViewWidth / 3f - 6f);
                bool twoColumns = false;
                if (width < 105) {
                    twoColumns = true;
                    width = Mathf.Max(20, EditorGUIUtility.currentViewWidth / 2f - 7f);
                }
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent("New", "Creates a new palette."), GUILayout.Width(width))) {
                    NewPalette();
                }
                GUI.enabled = otherPalette != null;
                if (GUILayout.Button(new GUIContent("Save", "Replace existing palette file."), GUILayout.Width(width))) {
                    if (EditorUtility.DisplayDialog("Confirmation", "Replace existing palette?", "Yes", "No")) {
                        SavePalette();
                        GUIUtility.ExitGUI();
                    }
                }
                GUI.enabled = true;
                if (twoColumns) {
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                }
                if (GUILayout.Button(new GUIContent("Save As New", "Saves current palette to a new file."), GUILayout.Width(width))) {
                    SaveAsNewPalette(palette);
                }
                if (!twoColumns) {
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                }
                GUI.enabled = otherPalette != null;
                if (GUILayout.Button(new GUIContent("Load", "Loads palette from file."), GUILayout.Width(width))) {
                    LoadPalette(otherPalette);
                    paletteChanges = true;
                }
                GUI.enabled = true;
                if (twoColumns) {
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                }
                if (GUILayout.Button(new GUIContent("Export LUT", "Generates a LUT (color look-up texture) which converts RGB colors to current palette. This LUT can be used with Beautify as a full-screen image effect color converter."), GUILayout.Width(width))) {
                    ExportLUT();
                }
                if (GUILayout.Button(new GUIContent("Generate Code", "Generates C# code with the palette color information."), GUILayout.Width(width))) {
                    ExportCode();
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent("Import ASE", "Imports a palette from a file in ASE format."), GUILayout.Width(width))) {
                    ImportASE();
                    paletteChanges = true;
                }
                if (GUILayout.Button(new GUIContent("Export Texture", "Exports a texture containing the palette colors."), GUILayout.Width(width))) {
                    ExportTexture();
                }
                EditorGUILayout.EndHorizontal();

            }

            if (viewMode == ViewMode.Help) {
                GUILayout.Label("Q U I C K   H E L P");

                EditorGUILayout.BeginVertical(GUI.skin.box);
                var headerStyle = new GUIStyle(EditorStyles.boldLabel);
                headerStyle.richText = true;
                var textStyle = new GUIStyle(EditorStyles.label);
                textStyle.wordWrap = true;
                GUILayout.Label("<size=24>Color Studio</size>\n© Kronnect", headerStyle);
                GUILayout.Label("For support and questions please visit kronnect.com forum.\nIf you like Color Studio, please rate it on the Asset Store.", textStyle);
                EditorGUILayout.EndHorizontal();
                if (GUILayout.Button("Open Welcome Center", EditorStyles.miniButton)) WelcomeWindow.ShowWelcomeCenter();
                EditorGUILayout.BeginVertical(GUI.skin.box);
                var titleStyle = new GUIStyle(EditorStyles.boldLabel);
                titleStyle.richText = true;
                GUILayout.Label("<size=12>Quick Interface Description</size>", titleStyle);
                var sectionStyle = new GUIStyle(EditorStyles.boldLabel);
                sectionStyle.normal.textColor = new Color(1f, 1f, 0.5f, 0.9f);
                GUILayout.Label("Color Wheel Tab", sectionStyle);
                GUILayout.Label("■ Use predefined color schemes to quickly generate palettes.\n■ Click several times on the scheme button to generate random combinations.\n■ The black dot represents the primary color.\n■ White dots are complementary colors.\n■ Drag the dots around the color wheel to customize your palette.\n■ Add additional colors holding CONTROL key and clicking on the color wheel.\n■ Remove additional colors double-clicking on the dots.", textStyle);
                EditorGUILayout.Separator();
                GUILayout.Label("Palette Tab", sectionStyle);
                GUILayout.Label("■ Expanded view of your current palette.\n■ Customize palette adjusting hue count, shades, brightness and color temperature (kelvin).\n■ Add/Remove custom colors.\n■ Load/save palettes and export to LUT and C# code.\n■ Import ASE palette files.", textStyle);
                EditorGUILayout.Separator();
                GUILayout.Label("Compact View Tab", sectionStyle);
                GUILayout.Label("■ Tiny view of your current palette. Useful to keep your palette visible along other tabs.", textStyle);
                EditorGUILayout.Separator();
                GUILayout.Label("Project Palettes Tab", sectionStyle);
                GUILayout.Label("■ Shows and manage existing palettes in the entire project.\n■ Quickly load a palette clicking on the name.\n■ Locate, duplicate or remove any palette.", textStyle);
                EditorGUILayout.Separator();
                GUILayout.Label("Coloring At Runtime", sectionStyle);
                GUILayout.Label("■ Add script Recolor to any GameObject or Sprite.", textStyle);
                EditorGUILayout.Separator();
                EditorGUILayout.Separator();
                GUILayout.Label(kronnect);
                EditorGUILayout.EndHorizontal();
            }


            EditorGUILayout.EndScrollView();

            if (e.type == EventType.MouseUp) {
                mouseDown = false;
                selectedKey = -1;
                draggingLightnessDot = false;
                if (undoGroupId != -1) {
                    Undo.CollapseUndoOperations(undoGroupId);
                    undoGroupId = -1;
                }
                // Clear cursor
                for (int k = 0; k < palette.keyColors.Length; k++) {
                    palette.keyColors[k].highlighted = false;
                }
                if (lightnessDotHighlighted) {
                    lightnessDotHighlighted = false;
                }
                UpdateCWMaterial();
                EditorUtility.SetDirty(palette);
            }

            if (issueRepaint) {
                Repaint();
            }
        }

        void Update() {
            if (cwMat != null) {
                float t = (float)((EditorApplication.timeSinceStartup - startAnimation) / 0.4);
                t = Mathf.Clamp01(t);
                cwMat.SetFloat(ShaderParams.AnimTime, t);
            }
            if (viewMode == ViewMode.ColorWheel) {
                Repaint();
            }

        }

        void UpdateCWMaterial() {

            if (cwMat == null) {
                cwMat = new Material(Shader.Find("Color Studio/ColorWheel"));
            }
            cwMat.DisableKeyword(ShaderParams.SKW_COMPLEMENTARY);
            cwMat.DisableKeyword(ShaderParams.SKW_SPLIT_COMPLEMENTARY);
            cwMat.DisableKeyword(ShaderParams.SKW_ANALOGOUS);
            cwMat.DisableKeyword(ShaderParams.SKW_TETRADIC);
            cwMat.DisableKeyword(ShaderParams.SKW_CUSTOM);
            switch (palette.scheme) {
                case ColorScheme.Complementary:
                case ColorScheme.Gradient:
                    cwMat.EnableKeyword(ShaderParams.SKW_COMPLEMENTARY);
                    break;
                case ColorScheme.SplitComplementary:
                case ColorScheme.Triadic:
                    cwMat.EnableKeyword(ShaderParams.SKW_SPLIT_COMPLEMENTARY);
                    break;
                case ColorScheme.Analogous:
                    cwMat.EnableKeyword(ShaderParams.SKW_ANALOGOUS);
                    break;
                case ColorScheme.Tetradic:
                case ColorScheme.Square:
                case ColorScheme.AccentedAnalogous:
                    cwMat.EnableKeyword(ShaderParams.SKW_TETRADIC);
                    break;
                case ColorScheme.Custom:
                    cwMat.EnableKeyword(ShaderParams.SKW_CUSTOM);
                    break;
            }
            cwMat.SetFloat(ShaderParams.CenterWidth, CENTER_WIDTH);
            cwMat.SetFloat(ShaderParams.Saturation, palette.saturation);
            cwMat.SetFloat(ShaderParams.MinBrightness, 0f);
            cwMat.SetFloat(ShaderParams.MaxBrightness, 1f);
            cwMat.SetFloat(ShaderParams.Lightness, palette.lightness);

            // Always provide primary hue and position to shader for outer ring rendering
            float primaryHue01 = ((palette.primaryAngle % PI2 + PI2) % PI2) / PI2;
            Vector2 primaryPos = GetCWPos(palette.primaryAngle, palette.primarySaturation);
            cwMat.SetVector(ShaderParams.Key0Pos, primaryPos);
            cwMat.SetFloat(ShaderParams.Key0Hue, primaryHue01);

            int customColorsCount = 0;
            for (int k = 0; k < palette.keyColors.Length; k++) {
                KeyColor c = palette.keyColors[k];
                if (c.visible) {
                    cwCustomColors[customColorsCount].x = c.pos.x;
                    cwCustomColors[customColorsCount].y = c.pos.y;
                    cwCustomColors[customColorsCount].z = c.type.dotColor() + (c.highlighted ? 256 : 0);
                    cwCustomColors[customColorsCount].w = c.hue;
                    customColorsCount++;
                }
            }
            // Append lightness dot on outer ring
            Vector2 lPos = draggingLightnessDot ? GetCWOuterRingPosByAngle(lightnessDotAngle) : GetCWLightnessDotPos(palette.primaryLightness, lightnessDotRightSide);
            cwCustomColors[customColorsCount].x = lPos.x;
            cwCustomColors[customColorsCount].y = lPos.y;
            cwCustomColors[customColorsCount].z = 6 + (lightnessDotHighlighted ? 256 : 0);
            cwCustomColors[customColorsCount].w = 0;
            customColorsCount++;
            cwMat.SetVectorArray(ShaderParams.KeyColorsData, cwCustomColors);
            cwMat.SetInt(ShaderParams.KeyColorsCount, customColorsCount);
        }

        bool SelectPrimaryInput() {
            float hue = 0, saturation = 0, lightness = 0;
            switch (primaryInputMode) {
                case ColorEncoding.RGB01: {
                        float[] rgb = StringTo3Floats(primaryInput);
                        if (rgb == null) return false;
                        Color color = new Color(rgb[0], rgb[1], rgb[2], 1f);
                        HSLColor hsl = ColorConversion.GetHSLFromRGB(color);
                        hue = hsl.h;
                        saturation = hsl.s;
                        lightness = hsl.l;
                        break;
                    }
                case ColorEncoding.RGB255: {
                        float[] rgb = StringTo3Floats(primaryInput);
                        if (rgb == null) return false;
                        Color color = new Color(rgb[0] / 255f, rgb[1] / 255f, rgb[2] / 255f, 1f);
                        HSLColor hsl = ColorConversion.GetHSLFromRGB(color);
                        hue = hsl.h;
                        saturation = hsl.s;
                        lightness = hsl.l;
                        break;
                    }
                case ColorEncoding.HSL: {
                        float[] hsl = StringTo3Floats(primaryInput);
                        if (hsl == null || hsl.Length == 0) return false;
                        hue = hsl[0];
                        saturation = hsl[1];
                        lightness = hsl[2];
                        break;
                    }
                case ColorEncoding.HEX: {
                        if (!ColorConversion.GetColorFromHex(primaryInput, out Color color)) return false;
                        HSLColor hsl = ColorConversion.GetHSLFromRGB(color);
                        hue = hsl.h;
                        saturation = hsl.s;
                        lightness = hsl.l;
                        break;
                    }
            }
            // Set global palette saturation/lightness from input
            Undo.RecordObject(palette, "Select Primary Input");
            palette.saturation = saturation;
            palette.lightness = lightness;
            palette.minBrightness = 0f;
            palette.maxBrightness = 1f;
            palette.SetKeyColor(0, KeyColorType.Primary, hue * PI2);
            UpdateKeyVisuals(0, hue * PI2, KeyColorType.Primary);

            clickedAngle = hue;
            return true;
        }

        void UpdatePrimaryInput() {
            float hue = ((palette.primaryAngle % PI2 + PI2) % PI2) / PI2;
            float saturation = palette.primarySaturation;
            float lightness = palette.primaryLightness;
            Color colorFromHSL = ColorConversion.GetColorFromHSL(hue, saturation, lightness);

            switch (primaryInputMode) {
                case ColorEncoding.RGB01:
                    primaryInput = colorFromHSL.r.ToString("F3", CultureInfo.InvariantCulture) + ", " + colorFromHSL.g.ToString("F3", CultureInfo.InvariantCulture) + ", " + colorFromHSL.b.ToString("F3", CultureInfo.InvariantCulture);
                    break;
                case ColorEncoding.RGB255:
                    primaryInput = Mathf.RoundToInt(colorFromHSL.r * 255).ToString(CultureInfo.InvariantCulture) + ", " + Mathf.RoundToInt(colorFromHSL.g * 255).ToString(CultureInfo.InvariantCulture) + ", " + Mathf.RoundToInt(colorFromHSL.b * 255).ToString(CultureInfo.InvariantCulture);
                    break;
                case ColorEncoding.HEX:
                    primaryInput = "#" + Mathf.RoundToInt(colorFromHSL.r * 255).ToString("X2") + Mathf.RoundToInt(colorFromHSL.g * 255).ToString("X2") + Mathf.RoundToInt(colorFromHSL.b * 255).ToString("X2");
                    break;
                case ColorEncoding.HSL:
                    primaryInput = hue.ToString("F3", CultureInfo.InvariantCulture) + ", " + saturation.ToString("F3", CultureInfo.InvariantCulture) + ", " + lightness.ToString("F3", CultureInfo.InvariantCulture);
                    break;
            }
        }

        void UpdateSelectionInput() {
            Color color = selectionColor;
            switch (selectionInputMode) {
                case ColorEncoding.RGB01:
                    selectionInput = color.r.ToString("F3", CultureInfo.InvariantCulture) + ", " + color.g.ToString("F3", CultureInfo.InvariantCulture) + ", " + color.b.ToString("F3", CultureInfo.InvariantCulture);
                    break;
                case ColorEncoding.RGB255:
                    selectionInput = Mathf.RoundToInt(color.r * 255).ToString(CultureInfo.InvariantCulture) + ", " + Mathf.RoundToInt(color.g * 255).ToString(CultureInfo.InvariantCulture) + ", " + Mathf.RoundToInt(color.b * 255).ToString(CultureInfo.InvariantCulture);
                    break;
                case ColorEncoding.HEX:
                    selectionInput = "#" + Mathf.RoundToInt(color.r * 255).ToString("X2") + Mathf.RoundToInt(color.g * 255).ToString("X2") + Mathf.RoundToInt(color.b * 255).ToString("X2");
                    break;
                case ColorEncoding.HSL:
                    HSLColor hsl = ColorConversion.GetHSLFromRGB(color);
                    float hue = hsl.h;
                    float saturation = hsl.s;
                    float lightness = hsl.l;
                    selectionInput = hue.ToString("F3", CultureInfo.InvariantCulture) + ", " + saturation.ToString("F3", CultureInfo.InvariantCulture) + ", " + lightness.ToString("F3", CultureInfo.InvariantCulture);
                    break;
            }
        }

        Vector2 GetCWPos(float angle) {
            Vector2 pos = new Vector3();
            pos.x = Mathf.Cos(angle);
            pos.y = Mathf.Sin(angle);
            float d = 0.25f + CENTER_WIDTH * 0.5f;
            pos.x = 0.5f - pos.x * d;
            pos.y = 0.5f - pos.y * d;
            return pos;
        }

        // Overload: position using saturation to place the key in middle ring
        Vector2 GetCWPos(float angle, float saturation) {
            Vector2 pos = new Vector3();
            pos.x = Mathf.Cos(angle);
            pos.y = Mathf.Sin(angle);
            float inner = CENTER_WIDTH; // matches _CenterWidth in shader
            // Middle ring in shader uses light ∈ [0..0.32] mapped from radius. sat = light/0.32
            float r = inner + Mathf.Clamp01(saturation) * 0.32f * (1f - inner);
            pos.x = 0.5f - pos.x * r;
            pos.y = 0.5f - pos.y * r;
            return pos;
        }

        // Position for the lightness dot on the outer ring, vertically mapped: top=0, bottom=1
        Vector2 GetCWLightnessDotPos(float lightness, bool rightSide) {
            float inner = CENTER_WIDTH + 0.32f * (1f - CENTER_WIDTH);
            float outer = 0.5f;
            float r = (inner + outer) * 0.5f;
            float sinTheta = Mathf.Clamp(1f - 2f * lightness, -1f, 1f);
            float cosTheta = Mathf.Sqrt(Mathf.Max(1f - sinTheta * sinTheta, 0f));
            if (!rightSide) cosTheta = -cosTheta;
            float x = 0.5f + r * cosTheta;
            float y = 0.5f + r * sinTheta;
            return new Vector2(x, y);
        }

        // Position on the outer ring by angle
        Vector2 GetCWOuterRingPosByAngle(float angle) {
            float inner = CENTER_WIDTH + 0.32f * (1f - CENTER_WIDTH);
            float outer = 0.5f;
            float r = (inner + outer) * 0.5f;
            float x = 0.5f + r * Mathf.Cos(angle);
            float y = 0.5f + r * Mathf.Sin(angle);
            return new Vector2(x, y);
        }

        // Project cursor position to the outer ring for smooth lightness dragging
        Vector2 ProjectCursorToOuterRing(Vector2 cursorPos) {
            Vector2 fromCenter = cursorPos - new Vector2(0.5f, 0.5f);
            float angle = Mathf.Atan2(fromCenter.y, fromCenter.x);
            float inner = CENTER_WIDTH + 0.32f * (1f - CENTER_WIDTH);
            float outer = 0.5f;
            float r = (inner + outer) * 0.5f;
            // Clamp Y to valid orbit to avoid drifting due to numerical issues
            float sinTheta = Mathf.Sin(angle);
            float cosTheta = Mathf.Cos(angle);
            Vector2 projectedFromCenter = new Vector2(cosTheta, sinTheta) * r;
            return projectedFromCenter + new Vector2(0.5f, 0.5f);
        }

        void SetColorKeys() {
            SetColorKeys(palette);
        }


        void SetColorKeys(CSPalette palette) {
            selectionPos.x = -1;
            selectionInput = "";
            selectedCustomColor = -1;
            selectionColor.a = 0;
            startAnimation = (float)EditorApplication.timeSinceStartup;

            // Set primary color
            for (int k = 0; k < CSPalette.START_INDEX_CUSTOM_COLOR; k++) {
                palette.keyColors[k].visible = false;
            }

            if (palette.scheme != ColorScheme.Spectrum && palette.scheme != ColorScheme.Custom) {
                palette.SetKeyColor(0, KeyColorType.Primary, palette.primaryAngle);
                UpdateKeyVisuals(0, palette.primaryAngle, KeyColorType.Primary);
            } else {
                cwMat.SetVector(ShaderParams.Key0Pos, Vector3.zero);
            }

            switch (palette.scheme) {
                case ColorScheme.Monochromatic:
                case ColorScheme.Custom:
                    break;
                case ColorScheme.Complementary: {
                        float a1 = palette.primaryAngle + Mathf.PI;
                        palette.SetKeyColor(1, KeyColorType.Complementary, a1);
                        UpdateKeyVisuals(1, a1, KeyColorType.Complementary);
                    }
                    break;
                case ColorScheme.Gradient: {
                        float a1 = palette.splitAmount + Mathf.PI;
                        palette.SetKeyColor(1, KeyColorType.Complementary, a1);
                        UpdateKeyVisuals(1, a1, KeyColorType.Complementary);
                    }
                    break;
                case ColorScheme.SplitComplementary: {
                        float a1 = palette.primaryAngle + palette.splitAmount + Mathf.PI;
                        float a2 = palette.primaryAngle - palette.splitAmount + Mathf.PI;
                        palette.SetKeyColor(1, KeyColorType.Complementary, a1);
                        palette.SetKeyColor(2, KeyColorType.Complementary, a2);
                        UpdateKeyVisuals(1, a1, KeyColorType.Complementary);
                        UpdateKeyVisuals(2, a2, KeyColorType.Complementary);
                    }
                    break;
                case ColorScheme.Analogous: {
                        float a1 = palette.primaryAngle + palette.splitAmount;
                        float a2 = palette.primaryAngle - palette.splitAmount;
                        palette.SetKeyColor(1, KeyColorType.Complementary, a1);
                        palette.SetKeyColor(2, KeyColorType.Complementary, a2);
                        UpdateKeyVisuals(1, a1, KeyColorType.Complementary);
                        UpdateKeyVisuals(2, a2, KeyColorType.Complementary);
                    }
                    break;
                case ColorScheme.Triadic: {
                        float a1 = palette.primaryAngle + Mathf.PI * 2f / 3f;
                        float a2 = palette.primaryAngle - Mathf.PI * 2f / 3f;
                        palette.SetKeyColor(1, KeyColorType.Complementary, a1);
                        palette.SetKeyColor(2, KeyColorType.Complementary, a2);
                        UpdateKeyVisuals(1, a1, KeyColorType.Complementary);
                        UpdateKeyVisuals(2, a2, KeyColorType.Complementary);
                    }
                    break;
                case ColorScheme.Tetradic: {
                        float a1 = palette.primaryAngle + palette.splitAmount;
                        float a2 = palette.primaryAngle + Mathf.PI;
                        float a3 = palette.primaryAngle + palette.splitAmount + Mathf.PI;
                        palette.SetKeyColor(1, KeyColorType.Complementary, a1);
                        palette.SetKeyColor(2, KeyColorType.Complementary, a2);
                        palette.SetKeyColor(3, KeyColorType.Complementary, a3);
                        UpdateKeyVisuals(1, a1, KeyColorType.Complementary);
                        UpdateKeyVisuals(2, a2, KeyColorType.Complementary);
                        UpdateKeyVisuals(3, a3, KeyColorType.Complementary);
                    }
                    break;
                case ColorScheme.Square: {
                        float a1 = palette.primaryAngle + HALF_PI;
                        float a2 = palette.primaryAngle + Mathf.PI;
                        float a3 = palette.primaryAngle + HALF_PI + Mathf.PI;
                        palette.SetKeyColor(1, KeyColorType.Complementary, a1);
                        palette.SetKeyColor(2, KeyColorType.Complementary, a2);
                        palette.SetKeyColor(3, KeyColorType.Complementary, a3);
                        UpdateKeyVisuals(1, a1, KeyColorType.Complementary);
                        UpdateKeyVisuals(2, a2, KeyColorType.Complementary);
                        UpdateKeyVisuals(3, a3, KeyColorType.Complementary);
                    }
                    break;
                case ColorScheme.AccentedAnalogous: {
                        float a1 = palette.primaryAngle + palette.splitAmount;
                        float a2 = palette.primaryAngle + Mathf.PI;
                        float a3 = palette.primaryAngle - palette.splitAmount;
                        palette.SetKeyColor(1, KeyColorType.Complementary, a1);
                        palette.SetKeyColor(2, KeyColorType.Complementary, a2);
                        palette.SetKeyColor(3, KeyColorType.Complementary, a3);
                        UpdateKeyVisuals(1, a1, KeyColorType.Complementary);
                        UpdateKeyVisuals(2, a2, KeyColorType.Complementary);
                        UpdateKeyVisuals(3, a3, KeyColorType.Complementary);
                    }
                    break;
            }

            // Ensure dot positions match current saturation
            SyncKeyPositionsWithSaturation();
            UpdateCWMaterial();
            palette.BuildHueColors();
        }

        void AddCustomColor(float hue) {
            for (int k = CSPalette.START_INDEX_CUSTOM_COLOR; k < palette.keyColors.Length; k++) {
                if (!palette.keyColors[k].visible) {
                    palette.SetKeyColor(k, KeyColorType.Custom, hue);
                    UpdateKeyVisuals(k, hue, KeyColorType.Custom);
                    break;
                }
            }
            SyncKeyPositionsWithSaturation();
            UpdateCWMaterial();
            palette.BuildHueColors();
        }

        void AddCustomColor(Color color) {
            for (int k = CSPalette.START_INDEX_CUSTOM_COLOR; k < palette.keyColors.Length; k++) {
                if (!palette.keyColors[k].visible) {
                    HSLColor hsl = ColorConversion.GetHSLFromRGB(color);
                    float angle = hsl.h * PI2;
                    palette.SetKeyColor(k, KeyColorType.Custom, angle);
                    UpdateKeyVisuals(k, angle, KeyColorType.Custom);
                    break;
                }
            }
            SyncKeyPositionsWithSaturation();
            UpdateCWMaterial();
            palette.BuildHueColors();
        }


        void UpdateKeyVisuals(int index, float angle, KeyColorType keyColorType) {
            Vector2 prevPos = palette.keyColors[index].pos;
            // Position all keys (including custom) using current palette saturation so dots reflect saturation on the middle ring
            Vector2 pos = GetCWPos(angle, palette.saturation);
            palette.keyColors[index].pos = pos;
            if (prevPos.x == 0) {
                prevPos = pos;
            }
            cwMat.SetVector("_Key" + index + "PosPrev", prevPos);
            cwMat.SetVector("_Key" + index + "Pos", pos);
        }


        void ClearCustomColors() {
            for (int k = CSPalette.START_INDEX_CUSTOM_COLOR; k < palette.keyColors.Length; k++) {
                palette.keyColors[k].visible = false;
            }
            SyncKeyPositionsWithSaturation();
            UpdateCWMaterial();
            palette.BuildHueColors();
        }

        // Positions all visible keys according to their angle and current palette saturation
        void SyncKeyPositionsWithSaturation() {
            if (palette == null || palette.keyColors == null) return;
            for (int i = 0; i < palette.keyColors.Length; i++) {
                if (palette.keyColors[i].visible) {
                    palette.keyColors[i].pos = GetCWPos(palette.keyColors[i].angle, palette.saturation);
                }
            }
        }

        string GetExportsPath(string subfolder) {
            string path = "Assets/Color Studio/" + subfolder;
            Directory.CreateDirectory(path);
            return path;
        }

        void FindProjectPalettes() {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(CSPalette).Name);
            List<CSPalette> found = new List<CSPalette>();
            projectPalettes = new CSPalette[guids.Length];
            for (int i = 0; i < guids.Length; i++) {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                CSPalette pal = projectPalettes[i] = AssetDatabase.LoadAssetAtPath<CSPalette>(path);
                if (pal == null || pal == palette) continue;
                if (pal.material == null) {
                    pal.UpdateMaterial();
                }
                pal.material = Instantiate<Material>(palette.material);
                pal.BuildHueColors();
                found.Add(pal);
            }
            projectPalettes = found.ToArray();
            ResortProjectPalettes();
        }

        void ResortProjectPalettes() {
            System.Array.Sort(projectPalettes, comparer);
        }

        int comparer(CSPalette a, CSPalette b) {
            return a.order.CompareTo(b.order);
        }

        void DrawConversionTools() {
            GUILayout.Label("C O N V E R T   C O L O R S");

            EditorGUIUtility.labelWidth = 120;
            GameObject prev = selectedObject;
            selectedObject = (GameObject)EditorGUILayout.ObjectField("GameObject", selectedObject, typeof(GameObject), true);
            if (selectedObject != prev) {
                SelectedObjectChanged();
                GUIUtility.ExitGUI();
            }
            Color prevColor = selectedObjectColor;
            selectedObjectColor = EditorGUILayout.ColorField("Color", selectedObjectColor);
            if (nearestColor.a > 0) {
                EditorGUILayout.ColorField("     Suggested", nearestColor);
            }
            if (selectedObjectColor != prevColor) {
                nearestColor = palette.GetNearestColor(selectedObjectColor, colorMatchMode, interpolate);
            }
            EditorGUILayout.Separator();
            Texture2D prevTexture = referenceTexture;
            referenceTexture = (Texture2D)EditorGUILayout.ObjectField("Texture", referenceTexture, typeof(Texture2D), false);
            if (referenceTexture != prevTexture && referenceTexture != null) {
                referenceTexture.EnsureTextureIsReadable();
                nearestTexture = palette.GetNearestTexture(referenceTexture, colorMatchMode, interpolate);
            }
            if (nearestTexture != null) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(new GUIContent("     Suggested"), nearestTexture, typeof(Texture2D), false);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space();
                if (GUILayout.Button(new GUIContent("Save Suggested Texture", "Exports suggested texture to disk."))) {
                    ExportColoredTexture();
                }
                if (GUILayout.Button("Refresh")) {
                    nearestTexture = palette.GetNearestTexture(referenceTexture, colorMatchMode, interpolate);
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Separator();
            colorMatchMode = (ColorMatchMode)EditorGUILayout.EnumPopup("Match Mode", colorMatchMode);
            interpolate = EditorGUILayout.Toggle(new GUIContent("Interpolate", "Keeps original color luminance"), interpolate);

            EditorGUILayout.Separator();
            EditorGUILayout.HelpBox("To change colors of an object at runtime, add the 'Recolor' script to it instead.", MessageType.Info);

            EditorGUILayout.Separator();
            GUILayout.Label("C U R R E N T   P A L E T T E");
            palette.material.SetVector(ShaderParams.CursorPos, Vector3.left);


        }

        void SelectedObjectChanged() {
            referenceTexture = null;
            nearestTexture = null;
            nearestColor = Color.white;
            if (selectedObject != null) {
                Renderer r = selectedObject.GetComponent<Renderer>();
                if (r != null) {
                    Material mat = r.sharedMaterial;
                    if (mat != null) {
                        if (mat.HasProperty(ShaderParams.Color)) {
                            selectedObjectColor = mat.color;
                            nearestColor = palette.GetNearestColor(selectedObjectColor, colorMatchMode, interpolate);
                        }
                        if (r is SpriteRenderer) {
                            SpriteRenderer spr = (SpriteRenderer)r;
                            if (spr.sprite != null && spr.sprite.texture != null) {
                                referenceTexture = spr.sprite.texture;
                                nearestTexture = palette.GetNearestTexture(referenceTexture, colorMatchMode, interpolate);
                            }
                        } else if (mat.HasProperty(ShaderParams.MainTex)) {
                            // Ensure texture is readable
                            if (mat.mainTexture is Texture2D) {
                                referenceTexture = (Texture2D)mat.mainTexture;
                                nearestTexture = palette.GetNearestTexture(referenceTexture, colorMatchMode, interpolate);
                            }
                        }
                    }
                }
            }
        }

        float[] StringTo3Floats(string s) {
            float[] xyz = new float[3];
            try {
                string[] values = s.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (values.Length >= 3) {
                    if (float.TryParse(values[0], NumberStyles.Any, CultureInfo.InvariantCulture, out xyz[0]) && float.TryParse(values[1], NumberStyles.Any, CultureInfo.InvariantCulture, out xyz[1]) && float.TryParse(values[2], NumberStyles.Any, CultureInfo.InvariantCulture, out xyz[2])) {
                        return xyz;
                    }
                }
            } catch {
            }
            return null;
        }


        void AddSelectionInput() {
            switch (selectionInputMode) {
                case ColorEncoding.RGB01: AddRGB01(); break;
                case ColorEncoding.RGB255: AddRGB(); break;
                case ColorEncoding.HSL: AddHSL(); break;
                case ColorEncoding.HEX: AddHEX(); break;
            }
        }

        void AddRGB01() {
            float[] rgb = StringTo3Floats(selectionInput);
            if (rgb != null) {
                AddCustomColor(new Color(rgb[0], rgb[1], rgb[2]));
            }
        }

        void AddRGB() {
            float[] rgb = StringTo3Floats(selectionInput);
            if (rgb != null) {
                AddCustomColor(new Color(rgb[0] / 255, rgb[1] / 255, rgb[2] / 255));
            }
        }

        void AddHSL() {
            float[] hsl = StringTo3Floats(selectionInput);
            if (hsl != null) {
                Color color = ColorConversion.GetColorFromHSL(hsl[0], hsl[1], hsl[2]);
                AddCustomColor(color);
            }
        }

        void AddHEX() {
            Color color;
            if (ColorConversion.GetColorFromHex(selectionInput, out color)) {
                AddCustomColor(color);
            }
        }

        void DeleteColor() {
            if (selectedCustomColor >= 0) {
                Undo.RecordObject(this, "Delete Custom Color");
                palette.DeleteKeyColor(selectedCustomColor);
                selectedCustomColor = -1;
                SetColorKeys();
            }

        }

    }

}
