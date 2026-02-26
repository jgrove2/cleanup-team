using System;
using System.Collections.Generic;
using Godot;

/// <summary>
/// Node3D component that manages spawning and despawning item scenes on a character.
/// Add this node as a child of any character that has equipment. Assign the Skeleton3D
/// and one bone name per equipment slot in the Inspector, then call Initialize() from
/// the character root's _Ready() to bind the data layer.
/// </summary>
[GlobalClass]
public partial class EquipmentVisualComponent : Node3D
{
    [Export] public Skeleton3D Skeleton { get; set; }

    [ExportGroup("Hand Slots")]
    [Export] public string RightHandBone { get; set; } = "";
    [Export] public string LeftHandBone { get; set; } = "";

    [ExportGroup("Armor Slots")]
    [Export] public string HeadBone { get; set; } = "";
    [Export] public string BodyBone { get; set; } = "";
    [Export] public string LegsBone { get; set; } = "";
    [Export] public string FeetBone { get; set; } = "";

    [ExportGroup("Bag Slot")]
    [Export] public string BagBone { get; set; } = "";

    // Maps each slot to its BoneAttachment3D anchor in the skeleton.
    private readonly Dictionary<EquipmentSlot, BoneAttachment3D> _attachments = new();
    // Tracks live scene instances so they can be removed on unequip.
    private readonly Dictionary<EquipmentSlot, Node3D> _activeInstances = new();

    private EquipmentComponent _equipment;
    private Node _ownerCharacter;

    // Relates each slot to the exported bone-name property on this component.
    private Dictionary<EquipmentSlot, Func<string>> BuildSlotBoneMap() => new()
    {
        { EquipmentSlot.RightHand, () => RightHandBone },
        { EquipmentSlot.LeftHand,  () => LeftHandBone  },
        { EquipmentSlot.Head,      () => HeadBone      },
        { EquipmentSlot.Body,      () => BodyBone      },
        { EquipmentSlot.Legs,      () => LegsBone      },
        { EquipmentSlot.Feet,      () => FeetBone      },
        { EquipmentSlot.Bag,       () => BagBone       },
    };

    public override void _Ready()
    {
        if (Skeleton == null)
        {
            GD.PushWarning($"[EquipmentVisualComponent] No Skeleton3D assigned on '{GetParent()?.Name}'. Assign it in the Inspector.");
            return;
        }

        foreach (var (slot, getBone) in BuildSlotBoneMap())
        {
            string boneName = getBone();
            if (string.IsNullOrWhiteSpace(boneName)) continue;

            int boneIdx = Skeleton.FindBone(boneName);
            if (boneIdx == -1)
            {
                GD.PushWarning($"[EquipmentVisualComponent] Bone '{boneName}' not found in Skeleton for slot {slot}.");
                continue;
            }

            var attachment = new BoneAttachment3D
            {
                Name = $"Attachment_{slot}",
                BoneIdx = boneIdx,
            };
            Skeleton.AddChild(attachment);
            _attachments[slot] = attachment;
        }
    }

    /// <summary>
    /// Returns the currently spawned scene instance for the given equipment slot,
    /// or null if nothing is equipped there. Use this to retrieve an
    /// <see cref="IWeaponScene"/> from the swing state.
    /// </summary>
    public Node3D GetSpawnedNode(EquipmentSlot slot) =>
        _activeInstances.TryGetValue(slot, out var node) ? node : null;

    /// <summary>
    /// Binds this visual component to a character's EquipmentComponent data layer.
    /// Call this from the character root's _Ready() after creating / retrieving the
    /// EquipmentComponent so that any items already equipped are spawned immediately.
    /// </summary>
    /// <param name="equipment">The character's equipment data component.</param>
    /// <param name="ownerCharacter">
    /// The character node that owns this component. Passed to weapon scenes so
    /// the hitbox can guard against self-hits. Pass <c>null</c> to skip that feature.
    /// </param>
    public void Initialize(EquipmentComponent equipment, Node ownerCharacter = null)
    {
        _ownerCharacter = ownerCharacter;
        if (equipment == null) throw new ArgumentNullException(nameof(equipment));

        // Unsubscribe from any previously bound component.
        if (_equipment != null)
        {
            _equipment.OnItemEquipped -= SpawnItemScene;
            _equipment.OnItemUnequipped -= DespawnItemScene;
        }

        _equipment = equipment;
        _equipment.OnItemEquipped += SpawnItemScene;
        _equipment.OnItemUnequipped += DespawnItemScene;

        // Spawn anything that was already equipped before Initialize was called.
        SpawnAllCurrentlyEquipped();
    }

    // -------------------------------------------------------------------------
    // Internal helpers
    // -------------------------------------------------------------------------

    private void SpawnAllCurrentlyEquipped()
    {
        foreach (var (slot, item) in _equipment.EquippedArmor)
            SpawnItemScene(slot, item);

        foreach (var (slot, item) in _equipment.EquippedInHands)
        {
            // Two-handed items share one reference across both hand slots.
            // Only spawn on the right hand to avoid duplicate scene instances.
            if (slot == EquipmentSlot.LeftHand && _equipment.IsTwoHanded(EquipmentSlot.LeftHand))
                continue;

            SpawnItemScene(slot, item);
        }

        if (_equipment.EquippedBag != null)
            SpawnItemScene(EquipmentSlot.Bag, _equipment.EquippedBag);
    }

    private void SpawnItemScene(EquipmentSlot slot, BaseItem item)
    {
        if (item?.SceneTemplate == null) return;

        // For two-handed items the left-hand event is intentionally ignored here;
        // the scene is already (or will be) anchored to the right hand.
        if (slot == EquipmentSlot.LeftHand && _equipment != null && _equipment.IsTwoHanded(EquipmentSlot.LeftHand))
            return;

        if (!_attachments.TryGetValue(slot, out var attachment)) return;

        // Remove any existing scene in this slot before placing the new one.
        DespawnItemScene(slot);

        var instance = item.SceneTemplate.Instantiate<Node3D>();
        attachment.AddChild(instance);
        _activeInstances[slot] = instance;

        // Offset the weapon root so the named grip node sits exactly at the
        // bone attachment origin, aligning the hand with the intended hold point.
        string holdName = slot switch
        {
            EquipmentSlot.RightHand => "right_hand_hold",
            EquipmentSlot.LeftHand  => "left_hand_hold",
            _                       => null
        };
        if (holdName != null)
        {
            var holdNode = instance.GetNodeOrNull<Node3D>(holdName);
            if (holdNode != null)
                instance.Position = -holdNode.Position;
        }

        // If the spawned scene is a weapon scene, hand it its data and wielder reference.
        if (item is Weapon weapon && instance is IWeaponScene weaponScene)
            weaponScene.InitializeWeapon(weapon, _ownerCharacter);
    }

    private void DespawnItemScene(EquipmentSlot slot)
    {
        if (!_activeInstances.TryGetValue(slot, out var instance)) return;
        instance.QueueFree();
        _activeInstances.Remove(slot);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && _equipment != null)
        {
            _equipment.OnItemEquipped -= SpawnItemScene;
            _equipment.OnItemUnequipped -= DespawnItemScene;
        }
        base.Dispose(disposing);
    }
}
