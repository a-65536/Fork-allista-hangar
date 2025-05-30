﻿//   HangarPassage.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2016 Allis Tauri

using System.Collections.Generic;
using UnityEngine;
using AT_Utils;

namespace AtHangar
{
    public class HangarPassage : ControllableModuleBase
    {
        [KSPField] public bool HideInfo;
        [SerializeField] public ConfigNode ModuleConfig;

        public readonly Dictionary<string, PassageNode> Nodes = new Dictionary<string, PassageNode>();
        public bool Ready { get; protected set; }

        #region Setup
        public override string GetInfo()
        {
            if(HideInfo) return "";
            init_nodes();
            if(Nodes.Count == 0) return "";
            var info = "Vessels can pass through:\n";
            var nodes = new List<string>(Nodes.Keys);
            nodes.Sort(); 
            nodes.ForEach(n => info += string.Format("- {0}: {1:F2}m x {2:F2}m\n", 
                n, Nodes[n].Size.x, Nodes[n].Size.y));
            return info;
        }

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            //only save config for the first time
            if(ModuleConfig == null) 
                ModuleConfig = node;
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            early_setup(state);
            Setup();
            start_coroutines();
        }

        protected virtual void early_setup(StartState state) { init_nodes(); }

        void init_nodes()
        {
            Nodes.Clear();
            if(ModuleConfig == null) 
            { this.Log("ModuleConfig is null. THIS SHOULD NEVER HAPPEN!"); return; }
            foreach(ConfigNode n in ModuleConfig.GetNodes(PassageNode.NODE_NAME))
            {
                var pn = new PassageNode(part);
                pn.Load(n);
                Nodes.Add(pn.NodeID, pn);
            }
        }

        virtual public void Setup(bool reset = false) {}

        protected virtual void start_coroutines() { Ready = true; }
        #endregion

        #region Logistics
        public PassageNode GetNodeByPart(Part p)
        {
            foreach(var n in Nodes.Values)
            { if(n.OtherPart == p) return n; }
            return null;
        }

        public List<HangarPassage> ConnectedPassages(PassageNode requesting_node = null)
        {
            var this_node = requesting_node?.OtherNode;
            var C = new List<HangarPassage>{this};
            foreach(var pn in Nodes.Values)
            {
                if(pn == this_node) continue;
                var other_passage = pn.OtherPassage;
                if(other_passage != null)
                    C.AddRange(other_passage.ConnectedPassages(pn));
            }
            return C;
        }

        virtual public bool CanHold(PackedVessel vsl) { return true; }

        public bool CanTransferTo(PackedVessel vsl, HangarPassage other, PassageNode requesting_node = null)
        {
            if(!enabled) return false;
            if(this == other) return CanHold(vsl);
            var this_node = requesting_node?.OtherNode;
            bool can_transfer = false;
            foreach(var pn in Nodes.Values)
            {
                if(pn == this_node) continue;
                var other_passage = pn.CanPassThrough(vsl);
                if(other_passage != null) 
                    can_transfer = other_passage.CanTransferTo(vsl, other, pn);
                if(can_transfer) break;
            }
            return can_transfer;
        }

        public Part ConnectedPartWithModule<ModuleT>(PassageNode requesting_node = null)
            where ModuleT : PartModule
        {
            if(part.HasModule<ModuleT>()) return part;
            var this_node = requesting_node?.OtherNode;
            foreach(var pn in Nodes.Values)
            {
                if(pn == this_node) continue;
                var other_passage = pn.OtherPassage;
                if(other_passage == null) continue;
                var p = other_passage.ConnectedPartWithModule<ModuleT>(pn);
                if(p != null) return p;
            }
            return null;
        }
        #endregion
    }


    public class PartPassages : ConfigNodeObject
    {
        public readonly Dictionary<string, PassageNode> Nodes = new Dictionary<string, PassageNode>();

        protected Part part;



        public override void Load(ConfigNode node)
        {
            base.Load(node);
            Nodes.Clear();
            foreach(ConfigNode n in node.GetNodes(PassageNode.NODE_NAME))
            {
                var pn = new PassageNode(part);
                pn.Load(n);
                Nodes.Add(pn.NodeID, pn);
            }
        }
    }

    public class PassageNode : ConfigNodeObject
    {
        new public const string NODE_NAME = "PASSAGE_NODE";
        [Persistent] public string NodeID = "_none_";
        [Persistent] public Vector3 Size; //ConfigNode's bug: can't LoadObjectFromConfig if I use Vector2

        readonly Part part;
        AttachNode part_node;
        ModuleDockingNode docking_node;

        public PassageNode(Part part) { this.part = part; }

        public Part OtherPart
        {
            get
            {
                if(part_node != null && part_node.attachedPart != null) 
                    return part_node.attachedPart;
                if(docking_node != null && part.vessel != null) 
                    return part.vessel[docking_node.dockedPartUId];
                return null;
            }
        }

        HangarPassage get_other_passage()
        {
            var other_part = OtherPart;
            return other_part?.Modules.GetModule<HangarPassage>();
        }

        public HangarPassage OtherPassage 
        { 
            get 
            {
                var other_passage = get_other_passage();
                if(other_passage != null && 
                    other_passage.GetNodeByPart(part) != null)
                    return other_passage;
                return null;
            }
        }

        public PassageNode OtherNode
        {
            get 
            {
                var other_passage = get_other_passage();
                return other_passage?.GetNodeByPart(part);
            }
        }

        public Vector2 MinSize 
        { 
            get 
            { 
                var other_node = OtherNode;
                if(other_node == null) return Size;
                return new Vector2(Mathf.Min(Size.x, other_node.Size.x), 
                    Mathf.Min(Size.y, other_node.Size.y));
            }
        }

        public override void Load(ConfigNode node)
        {
            base.Load(node);
            part_node = part.FindAttachNode(NodeID);
            docking_node = part.Modules.GetModule<ModuleDockingNode>();
            if(docking_node != null && docking_node.referenceAttachNode != NodeID) docking_node = null;
        }

        public HangarPassage CanPassThrough(PackedVessel vsl)
        {
            var other_passage = get_other_passage();
            if(other_passage == null) return null;
            var other_node = other_passage.GetNodeByPart(part);
            if(other_node == null) return null;
            var size = new Vector2(Mathf.Min(Size.x, other_node.Size.x), 
                Mathf.Min(Size.y, other_node.Size.y));
            return vsl.metric.FitsSomehow(size)? other_passage : null;
        }
    }

    public class PassageUpdater : ModuleUpdater<HangarPassage>
    { 
        protected override void on_rescale(ModulePair<HangarPassage> mp, Scale scale)
        {
            mp.module.Setup(!scale.FirstTime);
            foreach(var key in new List<string>(mp.module.Nodes.Keys))
                mp.module.Nodes[key].Size = Vector3.Scale(mp.base_module.Nodes[key].Size, 
                                                          new Vector3(scale, scale, 1));
        }
    }
}

