using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom editor for CardSetData to provide helpful information and validation.
/// </summary>
[CustomEditor(typeof(CardSetData))]
public class CardSetDataEditor : Editor
{
    private SerializedProperty setNameProp;
    private SerializedProperty setDescriptionProp;
    private SerializedProperty setIconProp;
    private SerializedProperty cardSpritesProp;
    private SerializedProperty hiddenCardSpriteProp;

    private void OnEnable()
    {
        setNameProp = serializedObject.FindProperty("setName");
        setDescriptionProp = serializedObject.FindProperty("setDescription");
        setIconProp = serializedObject.FindProperty("setIcon");
        cardSpritesProp = serializedObject.FindProperty("cardSprites");
        hiddenCardSpriteProp = serializedObject.FindProperty("hiddenCardSprite");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Card Set Information", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(setNameProp);
        EditorGUILayout.PropertyField(setDescriptionProp);
        EditorGUILayout.PropertyField(setIconProp);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Card Sprites Configuration", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(cardSpritesProp, true);
        EditorGUILayout.PropertyField(hiddenCardSpriteProp);

        EditorGUILayout.Space();
        DrawInfoBox();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawInfoBox()
    {
        CardSetData cardSet = (CardSetData)target;
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Card Set Statistics", EditorStyles.boldLabel);
        
        EditorGUILayout.LabelField($"Total Card Sprites: {cardSet.CardCount}");
        
        if (cardSet.CardCount > 0)
        {
            string difficulty = CardSetUtility.GetDifficultyRecommendation(cardSet.CardCount);
            EditorGUILayout.LabelField($"Recommended Use: {difficulty}");
            
            Vector2Int maxGrid = CardSetUtility.CalculateMaxGridSize(cardSet.CardCount);
            EditorGUILayout.LabelField($"Maximum Grid Size: {maxGrid.x} x {maxGrid.y}");
        }

        EditorGUILayout.Space();

        // Validation warnings
        if (cardSet.CardCount == 0)
        {
            EditorGUILayout.HelpBox("No card sprites assigned! Add sprite references to use this card set.", MessageType.Warning);
        }

        if (cardSet.HiddenCardSprite == null || !cardSet.HiddenCardSprite.RuntimeKeyIsValid())
        {
            EditorGUILayout.HelpBox("No hidden card sprite assigned! This is required for the game to work properly.", MessageType.Warning);
        }

        if (cardSet.CardCount < 2)
        {
            EditorGUILayout.HelpBox("At least 2 unique card sprites are needed for a basic 2x2 game.", MessageType.Info);
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // Quick Actions
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Mark All Sprites as Addressable"))
        {
            MarkSpritesAsAddressable(cardSet);
        }
        
        if (GUILayout.Button("Validate Card Set"))
        {
            ValidateCardSet(cardSet);
        }

        EditorGUILayout.EndVertical();
    }

    private void MarkSpritesAsAddressable(CardSetData cardSet)
    {
        EditorGUILayout.HelpBox("Please mark sprites as Addressable manually through the Addressables Groups window.", MessageType.Info);
        Debug.Log("To mark sprites as Addressable:\n1. Select the sprite in the Project window\n2. Check 'Addressable' in the Inspector\n3. Set an appropriate address/label");
    }

    private void ValidateCardSet(CardSetData cardSet)
    {
        bool isValid = true;
        string validationReport = "Card Set Validation Report:\n";

        if (string.IsNullOrEmpty(cardSet.SetName))
        {
            validationReport += "⚠ Set Name is empty\n";
            isValid = false;
        }

        if (cardSet.CardCount == 0)
        {
            validationReport += "⚠ No card sprites assigned\n";
            isValid = false;
        }

        if (cardSet.HiddenCardSprite == null || !cardSet.HiddenCardSprite.RuntimeKeyIsValid())
        {
            validationReport += "⚠ Hidden card sprite is not set or invalid\n";
            isValid = false;
        }

        // Check for duplicate references
        var spriteRefs = new System.Collections.Generic.HashSet<string>();
        for (int i = 0; i < cardSet.CardSprites.Count; i++)
        {
            var sprite = cardSet.CardSprites[i];
            if (sprite == null || !sprite.RuntimeKeyIsValid())
            {
                validationReport += $"⚠ Card sprite at index {i} is null or invalid\n";
                isValid = false;
            }
            else
            {
                string key = sprite.AssetGUID;
                if (spriteRefs.Contains(key))
                {
                    validationReport += $"⚠ Duplicate sprite reference found at index {i}\n";
                    isValid = false;
                }
                else
                {
                    spriteRefs.Add(key);
                }
            }
        }

        if (isValid)
        {
            validationReport += "✓ Card Set is valid and ready to use!";
            Debug.Log(validationReport);
            EditorUtility.DisplayDialog("Validation Success", validationReport, "OK");
        }
        else
        {
            Debug.LogWarning(validationReport);
            EditorUtility.DisplayDialog("Validation Failed", validationReport, "OK");
        }
    }
}
