﻿// *****************************************************************************
// BSD 3-Clause License (https://github.com/ComponentFactory/Krypton/blob/master/LICENSE)
//  © Component Factory Pty Ltd, 2006 - 2016, All rights reserved.
// The software and associated documentation supplied hereunder are the 
//  proprietary information of Component Factory Pty Ltd, 13 Swallows Close, 
//  Mornington, Vic 3931, Australia and are supplied subject to license terms.
// 
//  Modifications by Peter Wagner(aka Wagnerp) & Simon Coghlan(aka Smurf-IV), et al. 2017 - 2021. All rights reserved. (https://github.com/Krypton-Suite/Standard-Toolkit)
//  Version 6.0.0  
// *****************************************************************************

using System.ComponentModel.Design;

namespace Krypton.Toolkit
{
    internal class KryptonGroupActionList : DesignerActionList
    {
        #region Instance Fields
        private readonly KryptonGroup _group;
        private readonly IComponentChangeService _service;
        #endregion

        #region Identity
        /// <summary>
        /// Initialize a new instance of the KryptonGroupActionList class.
        /// </summary>
        /// <param name="owner">Designer that owns this action list instance.</param>
        public KryptonGroupActionList(KryptonGroupDesigner owner) 
            : base(owner.Component)
        {
            // Remember the group instance
            _group = owner.Component as KryptonGroup;

            // Cache service used to notify when a property has changed
            _service = (IComponentChangeService)GetService(typeof(IComponentChangeService));
        }
        #endregion
        
        #region Public
        /// <summary>
        /// Gets and sets the group background style.
        /// </summary>
        public PaletteBackStyle GroupBackStyle
        {
            get => _group.GroupBackStyle;

            set
            {
                if (_group.GroupBackStyle != value)
                {
                    _service.OnComponentChanged(_group, null, _group.GroupBackStyle, value);
                    _group.GroupBackStyle = value;
                }
            }
        }

        /// <summary>
        /// Gets and sets the group border style.
        /// </summary>
        public PaletteBorderStyle GroupBorderStyle
        {
            get => _group.GroupBorderStyle;

            set 
            {
                if (_group.GroupBorderStyle != value)
                {
                    _service.OnComponentChanged(_group, null, _group.GroupBorderStyle, value);
                    _group.GroupBorderStyle = value;
                }
            }
        }

        /// <summary>
        /// Gets and sets the palette mode.
        /// </summary>
        public PaletteMode PaletteMode
        {
            get => _group.PaletteMode;

            set 
            {
                if (_group.PaletteMode != value)
                {
                    _service.OnComponentChanged(_group, null, _group.PaletteMode, value);
                    _group.PaletteMode = value;
                }
            }
        }
        #endregion

        #region Public Override
        /// <summary>
        /// Returns the collection of DesignerActionItem objects contained in the list.
        /// </summary>
        /// <returns>A DesignerActionItem array that contains the items in this list.</returns>
        public override DesignerActionItemCollection GetSortedActionItems()
        {
            // Create a new collection for holding the single item we want to create
            DesignerActionItemCollection actions = new DesignerActionItemCollection();

            // This can be null when deleting a control instance at design time
            if (_group != null)
            {
                // Add the list of panel specific actions
                actions.Add(new DesignerActionHeaderItem("Appearance"));
                actions.Add(new DesignerActionPropertyItem("GroupBackStyle", "Back style", "Appearance", "Background style"));
                actions.Add(new DesignerActionPropertyItem("GroupBorderStyle", "Border style", "Appearance", "Border style"));
                actions.Add(new DesignerActionHeaderItem("Visuals"));
                actions.Add(new DesignerActionPropertyItem("PaletteMode", "Palette", "Visuals", "Palette applied to drawing"));
            }
            
            return actions;
        }
        #endregion
    }
}
