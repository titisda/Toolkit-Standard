﻿#region BSD License
/*
 * 
 * Original BSD 3-Clause License (https://github.com/ComponentFactory/Krypton/blob/master/LICENSE)
 *  © Component Factory Pty Ltd, 2006 - 2016, (Version 4.5.0.0) All rights reserved.
 * 
 *  New BSD 3-Clause License (https://github.com/Krypton-Suite/Standard-Toolkit/blob/master/LICENSE)
 *  Modifications by Peter Wagner (aka Wagnerp) & Simon Coghlan (aka Smurf-IV), et al. 2017 - 2022. All rights reserved. 
 *  
 */
#endregion

namespace Krypton.Toolkit
{
    /// <summary>
    /// Displays a task dialog that allows the user to select a task based on the presented options.
    /// </summary>
    [ToolboxItem(false)]
    public class VisualTaskDialog : KryptonForm
    {
        #region Types
        /// <summary>
        /// Internal button used by the VisualTaskDialog
        /// </summary>
        [ToolboxItem(false)]
        public class MessageButton : KryptonButton
        {
            #region Instance Fields

            #endregion

            #region Identity
            /// <summary>
            /// Gets and sets the ignoring of Alt+F4
            /// </summary>
            public bool IgnoreAltF4 { get; set; }

            #endregion

            #region Protected
            /// <summary>
            /// Processes Windows messages.
            /// </summary>
            /// <param name="m">The Windows Message to process. </param>
            protected override void WndProc(ref Message m)
            {
                switch (m.Msg)
                {
                    case PI.WM_.KEYDOWN:
                    case PI.WM_.SYSKEYDOWN:
                        if (IgnoreAltF4)
                        {
                            // Extract the keys being pressed
                            Keys keys = (Keys)(int)m.WParam.ToInt64();

                            // If the user standard combination ALT + F4
                            if ((keys == Keys.F4) && ((ModifierKeys & Keys.Alt) == Keys.Alt))
                            {
                                // Eat the message, so standard window proc does not close the window
                                return;
                            }
                        }
                        break;
                }

                base.WndProc(ref m);
            }
            #endregion
        }
        #endregion

        #region Static Fields

        private const int BUTTON_GAP = 10;

        #endregion

        #region Instance Fields
        private KryptonTaskDialog _taskDialog;
        private readonly string _windowTitle;
        private readonly string _mainInstruction;
        private readonly string _content;
        private readonly KryptonMessageBoxIcon _mainIcon;
        private readonly Image _customMainIcon;
        private readonly KryptonTaskDialogCommandCollection _radioButtons;
        private readonly KryptonTaskDialogCommandCollection _commandButtons;
        private KryptonTaskDialogCommand _defaultRadioButton;
        private readonly TaskDialogButtons _commonButtons;
        private readonly TaskDialogButtons _defaultButton;
        private readonly KryptonMessageBoxIcon _footerIcon;
        private readonly Image _customFooterIcon;
        private readonly string _footerText;
        private readonly string _footerHyperlink;
        private readonly string _checkboxText;
        private bool _checkboxState;
        private readonly bool _allowDialogClose;

        // User Interface
        private KryptonPanel _panelMain;
        private KryptonPanel _panelIcon;
        private PictureBox _messageIcon;
        private KryptonPanel _panelMainText;
        private KryptonWrapLabel _messageText;
        private KryptonWrapLabel _messageContent;
        private KryptonTextBox _messageContentMultiline;
        private KryptonPanel _panelButtons;
        private MessageButton _buttonOK;
        private MessageButton _buttonYes;
        private MessageButton _buttonNo;
        private MessageButton _buttonCancel;
        private MessageButton _buttonRetry;
        private KryptonBorderEdge _panelButtonsBorderTop;
        private KryptonPanel _panelFooter;
        private KryptonLinkLabel _linkLabelFooter;
        private PictureBox _iconFooter;
        private KryptonWrapLabel _footerLabel;
        private KryptonBorderEdge _panelFooterBorderTop;
        private KryptonCheckBox _checkBox;
        private KryptonPanel _panelMainRadio;
        private KryptonPanel _panelMainCommands;
        private KryptonPanel _panelMainSpacer;
        private MessageButton _buttonClose;
        #endregion

        #region Identity
        /// <summary>
        /// Initialize a new instance of the VisualTaskDialog class.
        /// </summary>
        /// <param name="taskDialog">Reference to component with definition of content.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public VisualTaskDialog(KryptonTaskDialog taskDialog)
        {
            // Must provide a valid reference

            _taskDialog = taskDialog ??
                          throw new ArgumentNullException(nameof(taskDialog));

            // Initialize with task dialog values
            _windowTitle = taskDialog.WindowTitle;
            _mainInstruction = taskDialog.MainInstruction;
            _content = taskDialog.Content;
            _mainIcon = taskDialog.Icon;
            _customMainIcon = taskDialog.CustomIcon;
            _radioButtons = taskDialog.RadioButtons;
            _commandButtons = taskDialog.CommandButtons;
            _commonButtons = taskDialog.CommonButtons;
            _defaultRadioButton = taskDialog.DefaultRadioButton;
            _defaultButton = taskDialog.DefaultButton;
            _footerIcon = taskDialog.FooterIcon;
            _customFooterIcon = taskDialog.CustomFooterIcon;
            _footerText = taskDialog.FooterText;
            _footerHyperlink = taskDialog.FooterHyperlink;
            _checkboxText = taskDialog.CheckboxText;
            _checkboxState = taskDialog.CheckboxState;
            _allowDialogClose = taskDialog.AllowDialogClose;

            InitializeComponent();
            TextExtra = taskDialog.TextExtra;
            UpdateContents();
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_taskDialog != null)
                {
                    _taskDialog = null;
                }
            }

            base.Dispose(disposing);
        }
        #endregion

        #region Implementation
        private void UpdateContents()
        {
            // Update contents to match requirements
            UpdateText();
            UpdateIcon();
            UpdateRadioButtons();
            UpdateCommandButtons();
            UpdateButtons();
            UpdateCheckbox();
            UpdateFooter();
            UpdateChrome();

            // Finally calculate and set form sizing
            UpdateSizing();
        }

        private void UpdateText()
        {
            Text = _windowTitle;
            _messageText.Text = _mainInstruction;
            // If the content has more than 20 lines, use the multi-line text control
            if (string.IsNullOrEmpty(_content))
            {
                _messageContent.Text = string.Empty;
            }
            else if (_content.Length - _content.Replace("\n", string.Empty).Length > 20)
            {
                _messageContentMultiline.Text = _content;
                _messageContentMultiline.Visible = true;
                _messageContent.Visible = false;
            }
            else
            {
                _messageContent.Text = _content;
                _messageContentMultiline.Visible = false;
                _messageContent.Visible = true;
            }
        }

        private void UpdateIcon()
        {
            _panelIcon.Visible = true;

            // Always use the custom icon as the preferred option
            if (_customMainIcon != null)
            {
                _messageIcon.Image = _customMainIcon;
            }
            else
            {
                switch (_mainIcon)
                {
                    case KryptonMessageBoxIcon.None:
                        _panelIcon.Visible = false;
                        _panelMainText.Left -= _messageIcon.Right;
                        break;
                    case KryptonMessageBoxIcon.Hand:
                        _messageIcon.Image = VisualTaskDialogImageResources.HandSmall;
                        SystemSounds.Hand.Play();
                        break;
                    case KryptonMessageBoxIcon.Question:
                        _messageIcon.Image = VisualTaskDialogImageResources.QuestionSmall;
                        SystemSounds.Question.Play();
                        break;
                    case KryptonMessageBoxIcon.Exclamation:
                        _messageIcon.Image = VisualTaskDialogImageResources.WarningSmall;
                        SystemSounds.Exclamation.Play();
                        break;
                    case KryptonMessageBoxIcon.Asterisk:
                        _messageIcon.Image = VisualTaskDialogImageResources.AsteriskSmall;
                        SystemSounds.Asterisk.Play();
                        break;
                    case KryptonMessageBoxIcon.Stop:
                        _messageIcon.Image = VisualTaskDialogImageResources.StopSmall;
                        SystemSounds.Asterisk.Play();
                        break;
                    case KryptonMessageBoxIcon.Information:
                        _messageIcon.Image = VisualTaskDialogImageResources.InformationSmall;
                        SystemSounds.Asterisk.Play();
                        break;
                    case KryptonMessageBoxIcon.Warning:
                        _messageIcon.Image = VisualTaskDialogImageResources.WarningSmall;
                        SystemSounds.Exclamation.Play();
                        break;
                    case KryptonMessageBoxIcon.Error:
                        _messageIcon.Image = VisualTaskDialogImageResources.CriticalSmall;
                        SystemSounds.Hand.Play();
                        break;
                    case KryptonMessageBoxIcon.Shield:
                        _messageIcon.Image = IconUtilities.ScaleImage(SystemIcons.Shield.ToBitmap(), 16, 16);
                        break;
                    case KryptonMessageBoxIcon.WindowsLogo:
                        // Because Windows 11 displays a generic application icon,
                        // we need to rely on a image instead
                        if (Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= 22000)
                        {
                            _messageIcon.Image = VisualTaskDialogImageResources.Windows_11_Small;
                        }
                        // Windows 10
                        else if (Environment.OSVersion.Version.Major == 10 && Environment.OSVersion.Version.Build <= 19044 /* RTM - 21H2 */)
                        {
                            _messageIcon.Image = VisualTaskDialogImageResources.Windows_8_and_10_Logo_Small;
                        }
                        else
                        {
                            _messageIcon.Image = IconUtilities.ScaleImage(SystemIcons.WinLogo.ToBitmap(), 16, 16);
                        }
                        break;
                }
            }
        }

        private void UpdateRadioButtons()
        {
            if (_radioButtons.Count == 0)
            {
                _panelMainRadio.Visible = false;
            }
            else
            {
                _panelMainRadio.Controls.Clear();
                _panelMainRadio.Visible = true;

                Size maxButtonSize = Size.Empty;
                foreach (KryptonTaskDialogCommand command in _radioButtons)
                {
                    // Create and add a new radio button instance
                    KryptonRadioButton button = new()
                    {
                        LabelStyle = LabelStyle.NormalPanel
                    };
                    button.Values.Text = command.Text;
                    button.Values.ExtraText = command.ExtraText;
                    button.Values.Image = command.Image;
                    button.Values.ImageTransparentColor = command.ImageTransparentColor;
                    button.Enabled = command.Enabled;
                    button.CheckedChanged += OnRadioButtonCheckedChanged;
                    button.Tag = command;
                    if (_defaultRadioButton == command)
                    {
                        button.Checked = true;
                    }

                    _panelMainRadio.Controls.Add(button);

                    // Note that largest radio button encountered
                    Size buttonSize = button.GetPreferredSize(Size.Empty);
                    maxButtonSize.Width = Math.Max(maxButtonSize.Width, buttonSize.Width);
                    maxButtonSize.Height = Math.Max(maxButtonSize.Height, buttonSize.Height);
                }

                // Enforce a maximum width to the commands
                maxButtonSize.Width = Math.Min(Math.Max(maxButtonSize.Width, 150), 400);

                // Position the radio buttons in a vertical stack and size owning panel
                Point offset = new(BUTTON_GAP - 1, 2);
                foreach (KryptonRadioButton button in _panelMainRadio.Controls)
                {
                    button.Location = offset;
                    button.Size = maxButtonSize;
                    offset.Y += maxButtonSize.Height;
                }

                // Size to the contained command controls
                _panelMainRadio.Size = new Size(maxButtonSize.Width, offset.Y);
            }
        }

        private void UpdateCommandButtons()
        {
            if (_commandButtons.Count == 0)
            {
                _panelMainCommands.Visible = false;
            }
            else
            {
                _panelMainCommands.Controls.Clear();
                _panelMainCommands.Visible = true;

                Size maxButtonSize = Size.Empty;
                foreach (KryptonTaskDialogCommand command in _commandButtons)
                {
                    // Create and add a new button instance
                    KryptonButton button = new()
                    {
                        ButtonStyle = ButtonStyle.Command
                    };
                    button.StateCommon.Content.Image.ImageH = PaletteRelativeAlign.Near;
                    button.StateCommon.Content.ShortText.TextH = PaletteRelativeAlign.Near;
                    button.StateCommon.Content.LongText.TextH = PaletteRelativeAlign.Near;
                    button.Values.Text = command.Text;
                    button.Values.ExtraText = command.ExtraText;
                    button.Values.Image = command.Image;
                    button.Values.ImageTransparentColor = command.ImageTransparentColor;
                    button.Enabled = command.Enabled;
                    button.DialogResult = command.DialogResult;
                    button.Tag = command;
                    button.Click += OnCommandClicked;
                    _panelMainCommands.Controls.Add(button);

                    // Note that largest button encountered
                    Size buttonSize = button.GetPreferredSize(Size.Empty);
                    maxButtonSize.Width = Math.Max(maxButtonSize.Width, buttonSize.Width);
                    maxButtonSize.Height = Math.Max(maxButtonSize.Height, buttonSize.Height);
                }

                // Enforce a maximum width to the commands
                maxButtonSize.Width = Math.Min(Math.Max(maxButtonSize.Width, 150), 400);

                // Position the buttons in a vertical stack and size owning panel
                Point offset = new(BUTTON_GAP - 1, 2);
                foreach (KryptonButton button in _panelMainCommands.Controls)
                {
                    button.Location = offset;
                    button.Size = maxButtonSize;
                    offset.Y += maxButtonSize.Height;
                }

                // Size to the contained command controls
                _panelMainCommands.Size = new Size(maxButtonSize.Width, offset.Y);
            }
        }

        private void UpdateButtons()
        {
            MessageButton firstButton = null;
            MessageButton defaultButton = null;

            if ((_commonButtons & TaskDialogButtons.OK) == TaskDialogButtons.OK)
            {
                if ((_defaultButton & TaskDialogButtons.OK) == TaskDialogButtons.OK)
                {
                    defaultButton = _buttonOK;
                }

                firstButton = _buttonOK;
                _buttonOK.Text = KryptonManager.Strings.OK;
                _buttonOK.Visible = true;
            }
            else
            {
                _buttonOK.Visible = false;
            }

            if ((_commonButtons & TaskDialogButtons.Yes) == TaskDialogButtons.Yes)
            {
                if ((_defaultButton & TaskDialogButtons.Yes) == TaskDialogButtons.Yes)
                {
                    defaultButton = _buttonYes;
                }

                if (firstButton == null)
                {
                    firstButton = _buttonYes;
                }

                _buttonYes.Text = KryptonManager.Strings.Yes;
                _buttonYes.Visible = true;
            }
            else
            {
                _buttonYes.Visible = false;
            }

            if ((_commonButtons & TaskDialogButtons.No) == TaskDialogButtons.No)
            {
                if ((_defaultButton & TaskDialogButtons.No) == TaskDialogButtons.No)
                {
                    defaultButton = _buttonNo;
                }

                if (firstButton == null)
                {
                    firstButton = _buttonNo;
                }

                _buttonNo.Text = KryptonManager.Strings.No;
                _buttonNo.Visible = true;
            }
            else
            {
                _buttonNo.Visible = false;
            }

            if ((_commonButtons & TaskDialogButtons.Cancel) == TaskDialogButtons.Cancel)
            {
                if ((_defaultButton & TaskDialogButtons.Cancel) == TaskDialogButtons.Cancel)
                {
                    defaultButton = _buttonCancel;
                }

                if (firstButton == null)
                {
                    firstButton = _buttonCancel;
                }

                _buttonCancel.Text = KryptonManager.Strings.Cancel;
                _buttonCancel.Visible = true;
            }
            else
            {
                _buttonCancel.Visible = false;
            }

            if ((_commonButtons & TaskDialogButtons.Retry) == TaskDialogButtons.Retry)
            {
                if ((_defaultButton & TaskDialogButtons.Retry) == TaskDialogButtons.Retry)
                {
                    defaultButton = _buttonRetry;
                }

                if (firstButton == null)
                {
                    firstButton = _buttonRetry;
                }

                _buttonRetry.Text = KryptonManager.Strings.Retry;
                _buttonRetry.Visible = true;
            }
            else
            {
                _buttonRetry.Visible = false;
            }

            if ((_commonButtons & TaskDialogButtons.Close) == TaskDialogButtons.Close)
            {
                if ((_defaultButton & TaskDialogButtons.Close) == TaskDialogButtons.Close)
                {
                    defaultButton = _buttonClose;
                }

                if (firstButton == null)
                {
                    firstButton = _buttonClose;
                }

                _buttonClose.Text = KryptonManager.Strings.Close;
                _buttonClose.Visible = true;
            }
            else
            {
                _buttonClose.Visible = false;
            }

            if (defaultButton != null)
            {
                defaultButton.Select();
            }
            else
            {
                firstButton?.Select();
            }
        }

        private void UpdateCheckbox()
        {
            _checkBox.Checked = _checkboxState;
            _checkBox.Text = _checkboxText;
            _checkBox.Visible = !string.IsNullOrEmpty(_checkboxText);
        }

        private void UpdateFooter()
        {
            _iconFooter.Visible = true;

            // Always use the custom icon as the preferred option
            if (_customFooterIcon != null)
            {
                _iconFooter.Image = _customFooterIcon;
            }
            else
            {
                // TODO: These icons may need to be 16 x 16
                switch (_footerIcon)
                {
                    case KryptonMessageBoxIcon.None:
                        _iconFooter.Visible = false;
                        break;
                    case KryptonMessageBoxIcon.Question:
                        _iconFooter.Image = MessageBoxResources.Question;
                        break;
                    case KryptonMessageBoxIcon.Information:
                        _iconFooter.Image = MessageBoxResources.Information;
                        break;
                    case KryptonMessageBoxIcon.Warning:
                        _iconFooter.Image = MessageBoxResources.Warning;
                        break;
                    case KryptonMessageBoxIcon.Error:
                        _iconFooter.Image = MessageBoxResources.Critical;
                        break;
                    case KryptonMessageBoxIcon.Hand:
                        _iconFooter.Image = MessageBoxResources.Hand;
                        break;
                    case KryptonMessageBoxIcon.Exclamation:
                        _iconFooter.Image = MessageBoxResources.Warning;
                        break;
                    case KryptonMessageBoxIcon.Asterisk:
                        _iconFooter.Image = MessageBoxResources.Asterisk;
                        break;
                    case KryptonMessageBoxIcon.Stop:
                        _iconFooter.Image = MessageBoxResources.Stop;
                        break;
                    case KryptonMessageBoxIcon.Shield:
                        _iconFooter.Image = IconUtilities.ScaleImage(SystemIcons.Shield.ToBitmap(), 16, 16);
                        break;
                    case KryptonMessageBoxIcon.WindowsLogo:
                        // Because Windows 11 displays a generic application icon,
                        // we need to rely on a image instead
                        if (Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= 22000)
                        {
                            _iconFooter.Image = MessageBoxResources.Windows11;
                        }
                        // Windows 10
                        else if (Environment.OSVersion.Version.Major == 10 && Environment.OSVersion.Version.Build <= 19044 /* RTM - 21H2 */)
                        {
                            _iconFooter.Image = MessageBoxResources.Windows_8_and_10_Logo;
                        }
                        else
                        {
                            _iconFooter.Image = SystemIcons.WinLogo.ToBitmap();
                        }

                        break;
                }
            }

            _footerLabel.Text = _footerText;
            _linkLabelFooter.Text = _footerHyperlink;
        }

        private void UpdateChrome()
        {
            if (((_commonButtons & TaskDialogButtons.Cancel) == TaskDialogButtons.Cancel) || _allowDialogClose)
            {
                ControlBox = true;
            }
            else
            {
                ControlBox = false;
            }

            _buttonOK.IgnoreAltF4 = !ControlBox;
            _buttonYes.IgnoreAltF4 = !ControlBox;
            _buttonNo.IgnoreAltF4 = !ControlBox;
            _buttonCancel.IgnoreAltF4 = !ControlBox;
            _buttonRetry.IgnoreAltF4 = !ControlBox;
            _buttonClose.IgnoreAltF4 = !ControlBox;
        }

        private void UpdateSizing()
        {
            Size messageSizing = UpdateMainTextSizing();
            Size radioSizing = UpdateRadioSizing();
            Size commandSizing = UpdateCommandSizing();
            Size spacerSizing = UpdateSpacerSizing();
            Size iconSizing = UpdateIconSizing();
            Size buttonsSizing = UpdateButtonsSizing();
            Size footerSizing = UpdateFooterSizing();

            // Size of window is calculated from the client area
            ClientSize = new Size(iconSizing.Width + Math.Max(messageSizing.Width, Math.Max(commandSizing.Width, Math.Max(radioSizing.Width, Math.Max(buttonsSizing.Width, footerSizing.Width)))),
                                  Math.Max(iconSizing.Height, messageSizing.Height + radioSizing.Height + commandSizing.Height + spacerSizing.Height) + buttonsSizing.Height + footerSizing.Height);
        }

        private Size UpdateMainTextSizing()
        {
            Size messageContentSize;

            // Update size of the main instruction and content labels but applying a sensible maximum
            using (Graphics g = CreateGraphics())
            {
                // Find size of the labels when it has a maximum length of 400
                _messageText.UpdateFont();
                _messageContent.UpdateFont();
                _messageContentMultiline.Font = _messageContent.Font;
                Size messageMainSize = g.MeasureString(_mainInstruction, _messageText.Font, 400).ToSize();
                messageContentSize = g.MeasureString(_content, _messageContent.Font, 400).ToSize();

                // Get the display size and make sure that the content size is not greater than 0.6 of display size
                Rectangle dispSize = Screen.GetWorkingArea(Location);

                var h = (int)Math.Min(messageContentSize.Height, dispSize.Height * 0.6);
                var w = (int)Math.Min(messageContentSize.Width, dispSize.Width * 0.6);
                Size sz = new(w, h);
                if (messageContentSize != sz)
                {
                    messageContentSize = sz;
                }

                // Work out DPI adjustment factor
                messageMainSize.Width = (int)(messageMainSize.Width * FactorDpiX);
                messageMainSize.Height = (int)(messageMainSize.Height * FactorDpiY);
                messageContentSize.Width = (int)(messageContentSize.Width * FactorDpiX);
                messageContentSize.Height = (int)(messageContentSize.Height * FactorDpiY);

                // Always add on an extra 5 pixels as sometimes the measure size does not draw the last 
                // character it contains, this ensures there is always definitely enough space for it all
                messageMainSize.Width += 5;
                messageContentSize.Width += 5;
                _messageText.Size = messageMainSize;
                _messageContent.Size = messageContentSize;
                _messageContentMultiline.Size = messageContentSize;
            }

            // Resize panel containing the main text
            Padding panelMessagePadding = _panelMainText.Padding;
            _panelMainText.Width = Math.Max(_messageText.Size.Width, messageContentSize.Width) + panelMessagePadding.Horizontal;
            _panelMainText.Height = _messageText.Size.Height + messageContentSize.Height + panelMessagePadding.Vertical + BUTTON_GAP;

            // Position the content label below the main label
            _messageContent.Location = new Point(_messageText.Left + 2, _messageText.Bottom);
            _messageContentMultiline.Location = _messageContent.Location;
            return _panelMainText.Size;
        }

        private Size UpdateIconSizing() => _messageIcon.Image == null ? Size.Empty : _panelIcon.Size;

        private Size UpdateRadioSizing()
        {
            _panelMainRadio.Location = new Point(_panelMainText.Left, _panelMainText.Top + _panelMainText.Height);

            if (_radioButtons.Count == 0)
            {
                _panelMainRadio.Size = Size.Empty;
                return Size.Empty;
            }

            return new Size(_panelMainRadio.Size.Width + BUTTON_GAP + 2, _panelMainRadio.Size.Height);
        }

        private Size UpdateCommandSizing()
        {
            _panelMainCommands.Location = new Point(_panelMainRadio.Left, _panelMainRadio.Top + _panelMainRadio.Height);

            if (_commandButtons.Count == 0)
            {
                _panelMainCommands.Size = Size.Empty;
                return Size.Empty;
            }

            return new Size(_panelMainCommands.Size.Width + BUTTON_GAP + 2, _panelMainCommands.Size.Height);
        }

        private Size UpdateSpacerSizing()
        {
            _panelMainSpacer.Location = new Point(_panelMainCommands.Left, _panelMainCommands.Top + _panelMainCommands.Height);
            return _panelMainSpacer.Size;
        }

        private Size UpdateButtonsSizing()
        {
            var numButtons = 0;
            Size maxButtonSize = Size.Empty;

            // Find the size of the largest button we need
            if ((_commonButtons & TaskDialogButtons.Close) == TaskDialogButtons.Close)
            {
                numButtons++;
                Size buttonCancelSize = _buttonClose.GetPreferredSize(Size.Empty);
                maxButtonSize.Width = Math.Max(maxButtonSize.Width, buttonCancelSize.Width + BUTTON_GAP);
                maxButtonSize.Height = Math.Max(maxButtonSize.Height, buttonCancelSize.Height);
            }

            if ((_commonButtons & TaskDialogButtons.Retry) == TaskDialogButtons.Retry)
            {
                numButtons++;
                Size buttonRetrySize = _buttonRetry.GetPreferredSize(Size.Empty);
                maxButtonSize.Width = Math.Max(maxButtonSize.Width, buttonRetrySize.Width + BUTTON_GAP);
                maxButtonSize.Height = Math.Max(maxButtonSize.Height, buttonRetrySize.Height);
            }

            if ((_commonButtons & TaskDialogButtons.Cancel) == TaskDialogButtons.Cancel)
            {
                numButtons++;
                Size buttonCancelSize = _buttonCancel.GetPreferredSize(Size.Empty);
                maxButtonSize.Width = Math.Max(maxButtonSize.Width, buttonCancelSize.Width + BUTTON_GAP);
                maxButtonSize.Height = Math.Max(maxButtonSize.Height, buttonCancelSize.Height);
            }

            if ((_commonButtons & TaskDialogButtons.No) == TaskDialogButtons.No)
            {
                numButtons++;
                Size buttonNoSize = _buttonNo.GetPreferredSize(Size.Empty);
                maxButtonSize.Width = Math.Max(maxButtonSize.Width, buttonNoSize.Width + BUTTON_GAP);
                maxButtonSize.Height = Math.Max(maxButtonSize.Height, buttonNoSize.Height);
            }

            if ((_commonButtons & TaskDialogButtons.Yes) == TaskDialogButtons.Yes)
            {
                numButtons++;
                Size buttonYesSize = _buttonYes.GetPreferredSize(Size.Empty);
                maxButtonSize.Width = Math.Max(maxButtonSize.Width, buttonYesSize.Width + BUTTON_GAP);
                maxButtonSize.Height = Math.Max(maxButtonSize.Height, buttonYesSize.Height);
            }

            if ((_commonButtons & TaskDialogButtons.OK) == TaskDialogButtons.OK)
            {
                numButtons++;
                Size buttonOKSize = _buttonOK.GetPreferredSize(Size.Empty);
                maxButtonSize.Width = Math.Max(maxButtonSize.Width, buttonOKSize.Width + BUTTON_GAP);
                maxButtonSize.Height = Math.Max(maxButtonSize.Height, buttonOKSize.Height);
            }


            // Start positioning buttons from right edge
            var right = _panelButtons.Right - BUTTON_GAP;

            if ((_commonButtons & TaskDialogButtons.Close) == TaskDialogButtons.Close)
            {
                _buttonClose.Location = new Point(right - maxButtonSize.Width, BUTTON_GAP);
                _buttonClose.Size = maxButtonSize;
                right -= maxButtonSize.Width + BUTTON_GAP;
            }

            if ((_commonButtons & TaskDialogButtons.Retry) == TaskDialogButtons.Retry)
            {
                _buttonRetry.Location = new Point(right - maxButtonSize.Width, BUTTON_GAP);
                _buttonRetry.Size = maxButtonSize;
                right -= maxButtonSize.Width + BUTTON_GAP;
            }

            if ((_commonButtons & TaskDialogButtons.Cancel) == TaskDialogButtons.Cancel)
            {
                _buttonCancel.Location = new Point(right - maxButtonSize.Width, BUTTON_GAP);
                _buttonCancel.Size = maxButtonSize;
                right -= maxButtonSize.Width + BUTTON_GAP;
            }

            if ((_commonButtons & TaskDialogButtons.No) == TaskDialogButtons.No)
            {
                _buttonNo.Location = new Point(right - maxButtonSize.Width, BUTTON_GAP);
                _buttonNo.Size = maxButtonSize;
                right -= maxButtonSize.Width + BUTTON_GAP;
            }

            if ((_commonButtons & TaskDialogButtons.Yes) == TaskDialogButtons.Yes)
            {
                _buttonYes.Location = new Point(right - maxButtonSize.Width, BUTTON_GAP);
                _buttonYes.Size = maxButtonSize;
                right -= maxButtonSize.Width + BUTTON_GAP;
            }

            if ((_commonButtons & TaskDialogButtons.OK) == TaskDialogButtons.OK)
            {
                _buttonOK.Location = new Point(right - maxButtonSize.Width, BUTTON_GAP);
                _buttonOK.Size = maxButtonSize;
                right -= maxButtonSize.Width + BUTTON_GAP;
            }

            Size checkboxSize = Size.Empty;
            if (!string.IsNullOrEmpty(_checkboxText))
            {
                checkboxSize = _checkBox.GetPreferredSize(Size.Empty);
            }

            if (numButtons == 0)
            {
                if (checkboxSize.IsEmpty)
                {
                    _panelButtons.Visible = false;
                    return Size.Empty;
                }
                else
                {
                    _panelButtons.Visible = true;
                    _checkBox.Location = new Point(BUTTON_GAP, BUTTON_GAP);
                    return new Size(checkboxSize.Width + (BUTTON_GAP * 2), checkboxSize.Height + (BUTTON_GAP * 2));
                }
            }
            else
            {
                _panelButtons.Visible = true;

                Size panelButtonSize = new((maxButtonSize.Width * numButtons) + (BUTTON_GAP * (numButtons + 1)), maxButtonSize.Height + (BUTTON_GAP * 2));

                if (!checkboxSize.IsEmpty)
                {
                    panelButtonSize.Width += checkboxSize.Width;
                    panelButtonSize.Height = Math.Max(panelButtonSize.Height, checkboxSize.Height + (BUTTON_GAP * 2));
                    _checkBox.Location = new Point(BUTTON_GAP, (panelButtonSize.Height - checkboxSize.Height) / 2);
                }

                return panelButtonSize;
            }
        }

        private Size UpdateFooterSizing()
        {
            // Update size of the footer but applying a sensible maximum
            using Graphics g = CreateGraphics();
            // Find size of the labels when it has a maximum length of 400
            _footerLabel.UpdateFont();
            Size footerTextSize = g.MeasureString(_footerText, _footerLabel.Font, 200).ToSize();
            Size footerHyperlinkSize = g.MeasureString(_footerHyperlink, _footerLabel.Font, 200).ToSize();

            // Always add on an extra 5 pixels as sometimes the measure size does not draw the last 
            // character it contains, this ensures there is always definitely enough space for it all
            footerTextSize.Width += 5;
            footerHyperlinkSize.Width += 5;
            _footerLabel.Size = footerTextSize;
            _linkLabelFooter.Size = footerHyperlinkSize;

            // Find required size of the footer panel
            Size requiredSize = Size.Empty;

            if (!string.IsNullOrEmpty(_footerText))
            {
                requiredSize.Width += footerTextSize.Width;
                requiredSize.Height = footerTextSize.Height;
            }

            if (!string.IsNullOrEmpty(_footerHyperlink))
            {
                requiredSize.Width += footerHyperlinkSize.Width;
                requiredSize.Height = Math.Max(requiredSize.Height, footerHyperlinkSize.Height);
            }

            if ((_footerIcon != KryptonMessageBoxIcon.None) || (_customFooterIcon != null))
            {
                requiredSize.Width += _iconFooter.Width + BUTTON_GAP;
                requiredSize.Height = Math.Max(requiredSize.Height, _iconFooter.Size.Height);
            }

            if (requiredSize.Width > 0)
            {
                requiredSize.Width += BUTTON_GAP * 2;
                requiredSize.Height += BUTTON_GAP * 2;
            }

            // Do we have anything to show?
            _panelFooter.Visible = requiredSize.Width > 0;

            // Position the footer elements
            if (requiredSize.Width > 0)
            {
                _panelFooter.Size = requiredSize;
                var offset = BUTTON_GAP;

                if ((_footerIcon != KryptonMessageBoxIcon.None) || (_customFooterIcon != null))
                {
                    _iconFooter.Location = new Point(offset, (requiredSize.Height - _iconFooter.Height) / 2);
                    offset += _iconFooter.Width + (BUTTON_GAP / 2);
                }

                if (!string.IsNullOrEmpty(_footerText))
                {
                    _footerLabel.Location = new Point(offset, (requiredSize.Height - footerTextSize.Height) / 2);
                    offset += _footerLabel.Width - 8;
                }

                if (!string.IsNullOrEmpty(_footerHyperlink))
                {
                    _linkLabelFooter.Location = !string.IsNullOrEmpty(_footerText)
                        ? new Point(offset, _footerLabel.Location.Y - 1)
                        : new Point(offset, (requiredSize.Height - footerHyperlinkSize.Height) / 2);

                    offset += _footerLabel.Width;
                }
            }

            return requiredSize;
        }

        private void OnRadioButtonCheckedChanged(object sender, EventArgs e)
        {
            KryptonRadioButton button = (KryptonRadioButton)sender;
            _defaultRadioButton = (KryptonTaskDialogCommand)button.Tag;
            _taskDialog.DefaultRadioButton = _defaultRadioButton;
        }

        private void OnCommandClicked(object sender, EventArgs e)
        {
            Close();

            // Update the result code from the command button
            KryptonButton button = (KryptonButton)sender;
            DialogResult = button.DialogResult;

            // Invoke any event handlers from the command button
            KryptonTaskDialogCommand command = (KryptonTaskDialogCommand)button.Tag;
            command.PerformExecute();
        }

        private void OnTaskDialogFormClosing(object sender, FormClosingEventArgs e)
        {
            // If the dialog is being closed because of a user event then it would be either
            // Alt+F4, Hit the close chrome button or the Close common dialog button
            if (e.CloseReason == CloseReason.UserClosing)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

#pragma warning disable IDE1006 // Naming Styles
        private void checkBox_CheckedChanged(object sender, EventArgs e)
#pragma warning restore IDE1006 // Naming Styles
        {
            _checkboxState = _checkBox.Checked;
            if (_taskDialog != null)
            {
                _taskDialog.CheckboxState = _checkboxState;
            }
        }

        private void LinkLabelFooter_LinkClicked(object sender, EventArgs e) => _taskDialog?.RaiseFooterHyperlinkClicked();

        private void _buttonClose_Click(object sender, EventArgs e) => Close();

#pragma warning disable IDE1006 // Naming Styles
        private void button_keyDown(object sender, KeyEventArgs e)
#pragma warning restore IDE1006 // Naming Styles
        {
            // Escape key kills the dialog if we allow it to be closed
            if ((e.KeyCode == Keys.Escape) && ControlBox)
            {
                Close();
            }
            else
            {
                // Pressing Ctrl+C should copy message text into the clipboard
                if ((e.Modifiers == Keys.Control)
                    && (e.KeyCode == Keys.C))
                {
                    StringBuilder sb = new();

                    sb.AppendLine("---------------------------");
                    sb.AppendLine(_windowTitle);
                    sb.AppendLine("---------------------------");
                    sb.AppendLine(_mainInstruction);
                    sb.AppendLine("---------------------------");
                    sb.AppendLine(_content);
                    sb.AppendLine("---------------------------");
                    if (_buttonOK.Visible)
                    {
                        sb.Append(_buttonOK.Text);
                        sb.Append("   ");
                    }
                    if (_buttonYes.Visible)
                    {
                        sb.Append(_buttonYes.Text);
                        sb.Append("   ");
                    }
                    if (_buttonNo.Visible)
                    {
                        sb.Append(_buttonNo.Text);
                        sb.Append("   ");
                    }
                    if (_buttonCancel.Visible)
                    {
                        sb.Append(_buttonCancel.Text);
                        sb.Append("   ");
                    }
                    if (_buttonRetry.Visible)
                    {
                        sb.Append(_buttonRetry.Text);
                        sb.Append("   ");
                    }
                    if (_buttonClose.Visible)
                    {
                        sb.Append(_buttonClose.Text);
                        sb.Append("   ");
                    }
                    sb.AppendLine("");
                    sb.AppendLine("---------------------------");

                    Clipboard.SetText(sb.ToString(), TextDataFormat.Text);
                    Clipboard.SetText(sb.ToString(), TextDataFormat.UnicodeText);
                }
            }
        }

        private void InitializeComponent()
        {
            this._panelMain = new Krypton.Toolkit.KryptonPanel();
            this._panelMainSpacer = new Krypton.Toolkit.KryptonPanel();
            this._panelMainCommands = new Krypton.Toolkit.KryptonPanel();
            this._panelMainRadio = new Krypton.Toolkit.KryptonPanel();
            this._panelMainText = new Krypton.Toolkit.KryptonPanel();
            this._messageContent = new Krypton.Toolkit.KryptonWrapLabel();
            this._messageContentMultiline = new Krypton.Toolkit.KryptonTextBox();
            this._messageText = new Krypton.Toolkit.KryptonWrapLabel();
            this._panelIcon = new Krypton.Toolkit.KryptonPanel();
            this._messageIcon = new System.Windows.Forms.PictureBox();
            this._panelButtons = new Krypton.Toolkit.KryptonPanel();
            this._checkBox = new Krypton.Toolkit.KryptonCheckBox();
            this._panelButtonsBorderTop = new Krypton.Toolkit.KryptonBorderEdge();
            this._buttonOK = new Krypton.Toolkit.VisualTaskDialog.MessageButton();
            this._buttonYes = new Krypton.Toolkit.VisualTaskDialog.MessageButton();
            this._buttonNo = new Krypton.Toolkit.VisualTaskDialog.MessageButton();
            this._buttonRetry = new Krypton.Toolkit.VisualTaskDialog.MessageButton();
            this._buttonCancel = new Krypton.Toolkit.VisualTaskDialog.MessageButton();
            this._buttonClose = new Krypton.Toolkit.VisualTaskDialog.MessageButton();
            this._panelFooter = new Krypton.Toolkit.KryptonPanel();
            this._linkLabelFooter = new Krypton.Toolkit.KryptonLinkLabel();
            this._iconFooter = new System.Windows.Forms.PictureBox();
            this._footerLabel = new Krypton.Toolkit.KryptonWrapLabel();
            this._panelFooterBorderTop = new Krypton.Toolkit.KryptonBorderEdge();
            ((System.ComponentModel.ISupportInitialize)(this._panelMain)).BeginInit();
            this._panelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._panelMainSpacer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._panelMainCommands)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._panelMainRadio)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._panelMainText)).BeginInit();
            this._panelMainText.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._panelIcon)).BeginInit();
            this._panelIcon.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._messageIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._panelButtons)).BeginInit();
            this._panelButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._panelFooter)).BeginInit();
            this._panelFooter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._iconFooter)).BeginInit();
            this.SuspendLayout();
            // 
            // _panelMain
            // 
            this._panelMain.AutoSize = true;
            this._panelMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._panelMain.Controls.Add(this._panelMainSpacer);
            this._panelMain.Controls.Add(this._panelMainCommands);
            this._panelMain.Controls.Add(this._panelMainRadio);
            this._panelMain.Controls.Add(this._panelMainText);
            this._panelMain.Controls.Add(this._panelIcon);
            this._panelMain.Dock = System.Windows.Forms.DockStyle.Top;
            this._panelMain.Location = new System.Drawing.Point(0, 0);
            this._panelMain.Name = "_panelMain";
            this._panelMain.Size = new System.Drawing.Size(578, 72);
            this._panelMain.TabIndex = 0;
            // 
            // _panelMainSpacer
            // 
            this._panelMainSpacer.Location = new System.Drawing.Point(42, 59);
            this._panelMainSpacer.Name = "_panelMainSpacer";
            this._panelMainSpacer.Size = new System.Drawing.Size(10, 10);
            this._panelMainSpacer.TabIndex = 3;
            // 
            // _panelMainCommands
            // 
            this._panelMainCommands.AutoSize = true;
            this._panelMainCommands.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._panelMainCommands.Location = new System.Drawing.Point(208, 10);
            this._panelMainCommands.Name = "_panelMainCommands";
            this._panelMainCommands.Size = new System.Drawing.Size(0, 0);
            this._panelMainCommands.TabIndex = 2;
            // 
            // _panelMainRadio
            // 
            this._panelMainRadio.AutoSize = true;
            this._panelMainRadio.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._panelMainRadio.Location = new System.Drawing.Point(208, 32);
            this._panelMainRadio.Name = "_panelMainRadio";
            this._panelMainRadio.Size = new System.Drawing.Size(0, 0);
            this._panelMainRadio.TabIndex = 1;
            // 
            // _panelMainText
            // 
            this._panelMainText.AutoSize = true;
            this._panelMainText.Controls.Add(this._messageContent);
            this._panelMainText.Controls.Add(this._messageContentMultiline);
            this._panelMainText.Controls.Add(this._messageText);
            this._panelMainText.Location = new System.Drawing.Point(42, 0);
            this._panelMainText.Margin = new System.Windows.Forms.Padding(0);
            this._panelMainText.Name = "_panelMainText";
            this._panelMainText.Padding = new System.Windows.Forms.Padding(5, 5, 5, 0);
            this._panelMainText.Size = new System.Drawing.Size(407, 60);
            this._panelMainText.TabIndex = 0;
            // 
            // _messageContent
            // 
            this._messageContent.AutoSize = false;
            this._messageContent.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._messageContent.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(57)))), ((int)(((byte)(91)))));
            this._messageContent.Location = new System.Drawing.Point(6, 34);
            this._messageContent.Margin = new System.Windows.Forms.Padding(0);
            this._messageContent.Name = "_messageContent";
            this._messageContent.Size = new System.Drawing.Size(78, 15);
            this._messageContent.Text = "Content";
            // 
            // _messageContentMultiline
            // 
            this._messageContentMultiline.Location = new System.Drawing.Point(48, 45);
            this._messageContentMultiline.Multiline = true;
            this._messageContentMultiline.Name = "_messageContentMultiline";
            this._messageContentMultiline.ReadOnly = true;
            this._messageContentMultiline.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this._messageContentMultiline.Size = new System.Drawing.Size(351, 10);
            this._messageContentMultiline.TabIndex = 4;
            // 
            // _messageText
            // 
            this._messageText.AutoSize = false;
            this._messageText.Font = new System.Drawing.Font("Segoe UI", 13.5F, System.Drawing.FontStyle.Bold);
            this._messageText.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(57)))), ((int)(((byte)(91)))));
            this._messageText.LabelStyle = Krypton.Toolkit.LabelStyle.TitlePanel;
            this._messageText.Location = new System.Drawing.Point(5, 5);
            this._messageText.Margin = new System.Windows.Forms.Padding(0);
            this._messageText.Name = "_messageText";
            this._messageText.Size = new System.Drawing.Size(139, 27);
            this._messageText.Text = "Message Text";
            // 
            // _panelIcon
            // 
            this._panelIcon.AutoSize = true;
            this._panelIcon.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._panelIcon.Controls.Add(this._messageIcon);
            this._panelIcon.Location = new System.Drawing.Point(0, 0);
            this._panelIcon.Margin = new System.Windows.Forms.Padding(0);
            this._panelIcon.Name = "_panelIcon";
            this._panelIcon.Padding = new System.Windows.Forms.Padding(10, 10, 0, 10);
            this._panelIcon.Size = new System.Drawing.Size(42, 52);
            this._panelIcon.TabIndex = 0;
            // 
            // _messageIcon
            // 
            this._messageIcon.BackColor = System.Drawing.Color.Transparent;
            this._messageIcon.Location = new System.Drawing.Point(10, 10);
            this._messageIcon.Margin = new System.Windows.Forms.Padding(0);
            this._messageIcon.Name = "_messageIcon";
            this._messageIcon.Size = new System.Drawing.Size(32, 32);
            this._messageIcon.TabIndex = 0;
            this._messageIcon.TabStop = false;
            // 
            // _panelButtons
            // 
            this._panelButtons.Controls.Add(this._checkBox);
            this._panelButtons.Controls.Add(this._panelButtonsBorderTop);
            this._panelButtons.Controls.Add(this._buttonOK);
            this._panelButtons.Controls.Add(this._buttonYes);
            this._panelButtons.Controls.Add(this._buttonNo);
            this._panelButtons.Controls.Add(this._buttonRetry);
            this._panelButtons.Controls.Add(this._buttonCancel);
            this._panelButtons.Controls.Add(this._buttonClose);
            this._panelButtons.Dock = System.Windows.Forms.DockStyle.Top;
            this._panelButtons.Location = new System.Drawing.Point(0, 72);
            this._panelButtons.Margin = new System.Windows.Forms.Padding(0);
            this._panelButtons.Name = "_panelButtons";
            this._panelButtons.PanelBackStyle = Krypton.Toolkit.PaletteBackStyle.PanelAlternate;
            this._panelButtons.Size = new System.Drawing.Size(578, 46);
            this._panelButtons.TabIndex = 1;
            // 
            // _checkBox
            // 
            this._checkBox.Location = new System.Drawing.Point(12, 12);
            this._checkBox.Name = "_checkBox";
            this._checkBox.Size = new System.Drawing.Size(75, 20);
            this._checkBox.TabIndex = 0;
            this._checkBox.Values.Text = "checkBox";
            // 
            // _panelButtonsBorderTop
            // 
            this._panelButtonsBorderTop.BorderStyle = Krypton.Toolkit.PaletteBorderStyle.HeaderPrimary;
            this._panelButtonsBorderTop.Dock = System.Windows.Forms.DockStyle.Top;
            this._panelButtonsBorderTop.Location = new System.Drawing.Point(0, 0);
            this._panelButtonsBorderTop.Name = "_panelButtonsBorderTop";
            this._panelButtonsBorderTop.Size = new System.Drawing.Size(578, 1);
            this._panelButtonsBorderTop.Text = "kryptonBorderEdge1";
            // 
            // _buttonOK
            // 
            this._buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonOK.AutoSize = true;
            this._buttonOK.CornerRoundingRadius = -1F;
            this._buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._buttonOK.IgnoreAltF4 = false;
            this._buttonOK.Location = new System.Drawing.Point(469, 9);
            this._buttonOK.Margin = new System.Windows.Forms.Padding(0);
            this._buttonOK.MinimumSize = new System.Drawing.Size(50, 26);
            this._buttonOK.Name = "_buttonOK";
            this._buttonOK.Size = new System.Drawing.Size(50, 26);
            this._buttonOK.TabIndex = 1;
            this._buttonOK.Values.Text = "OK";
            // 
            // _buttonYes
            // 
            this._buttonYes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonYes.AutoSize = true;
            this._buttonYes.CornerRoundingRadius = -1F;
            this._buttonYes.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this._buttonYes.IgnoreAltF4 = false;
            this._buttonYes.Location = new System.Drawing.Point(369, 9);
            this._buttonYes.Margin = new System.Windows.Forms.Padding(0);
            this._buttonYes.MinimumSize = new System.Drawing.Size(50, 26);
            this._buttonYes.Name = "_buttonYes";
            this._buttonYes.Size = new System.Drawing.Size(50, 26);
            this._buttonYes.TabIndex = 2;
            this._buttonYes.Values.Text = "Yes";
            // 
            // _buttonNo
            // 
            this._buttonNo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonNo.AutoSize = true;
            this._buttonNo.CornerRoundingRadius = -1F;
            this._buttonNo.DialogResult = System.Windows.Forms.DialogResult.No;
            this._buttonNo.IgnoreAltF4 = false;
            this._buttonNo.Location = new System.Drawing.Point(319, 9);
            this._buttonNo.Margin = new System.Windows.Forms.Padding(0);
            this._buttonNo.MinimumSize = new System.Drawing.Size(50, 26);
            this._buttonNo.Name = "_buttonNo";
            this._buttonNo.Size = new System.Drawing.Size(50, 26);
            this._buttonNo.TabIndex = 3;
            this._buttonNo.Values.Text = "No";
            // 
            // _buttonRetry
            // 
            this._buttonRetry.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonRetry.AutoSize = true;
            this._buttonRetry.CornerRoundingRadius = -1F;
            this._buttonRetry.DialogResult = System.Windows.Forms.DialogResult.Retry;
            this._buttonRetry.IgnoreAltF4 = false;
            this._buttonRetry.Location = new System.Drawing.Point(419, 9);
            this._buttonRetry.Margin = new System.Windows.Forms.Padding(0);
            this._buttonRetry.MinimumSize = new System.Drawing.Size(50, 26);
            this._buttonRetry.Name = "_buttonRetry";
            this._buttonRetry.Size = new System.Drawing.Size(50, 26);
            this._buttonRetry.TabIndex = 5;
            this._buttonRetry.Values.Text = "Retry";
            // 
            // _buttonCancel
            // 
            this._buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonCancel.AutoSize = true;
            this._buttonCancel.CornerRoundingRadius = -1F;
            this._buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._buttonCancel.IgnoreAltF4 = false;
            this._buttonCancel.Location = new System.Drawing.Point(262, 9);
            this._buttonCancel.Margin = new System.Windows.Forms.Padding(0);
            this._buttonCancel.MinimumSize = new System.Drawing.Size(50, 26);
            this._buttonCancel.Name = "_buttonCancel";
            this._buttonCancel.Size = new System.Drawing.Size(57, 26);
            this._buttonCancel.TabIndex = 4;
            this._buttonCancel.Values.Text = "Cancel";
            // 
            // _buttonClose
            // 
            this._buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonClose.AutoSize = true;
            this._buttonClose.CornerRoundingRadius = -1F;
            this._buttonClose.IgnoreAltF4 = false;
            this._buttonClose.Location = new System.Drawing.Point(519, 9);
            this._buttonClose.Margin = new System.Windows.Forms.Padding(0);
            this._buttonClose.MinimumSize = new System.Drawing.Size(50, 26);
            this._buttonClose.Name = "_buttonClose";
            this._buttonClose.Size = new System.Drawing.Size(50, 26);
            this._buttonClose.TabIndex = 6;
            this._buttonClose.Values.Text = "Close";
            // 
            // _panelFooter
            // 
            this._panelFooter.Controls.Add(this._linkLabelFooter);
            this._panelFooter.Controls.Add(this._iconFooter);
            this._panelFooter.Controls.Add(this._footerLabel);
            this._panelFooter.Controls.Add(this._panelFooterBorderTop);
            this._panelFooter.Dock = System.Windows.Forms.DockStyle.Top;
            this._panelFooter.Location = new System.Drawing.Point(0, 118);
            this._panelFooter.Name = "_panelFooter";
            this._panelFooter.PanelBackStyle = Krypton.Toolkit.PaletteBackStyle.PanelAlternate;
            this._panelFooter.Size = new System.Drawing.Size(578, 49);
            this._panelFooter.TabIndex = 2;
            // 
            // _linkLabelFooter
            // 
            this._linkLabelFooter.Location = new System.Drawing.Point(127, 11);
            this._linkLabelFooter.Name = "_linkLabelFooter";
            this._linkLabelFooter.Size = new System.Drawing.Size(110, 20);
            this._linkLabelFooter.TabIndex = 0;
            this._linkLabelFooter.Values.Text = "kryptonLinkLabel1";
            // 
            // _iconFooter
            // 
            this._iconFooter.BackColor = System.Drawing.Color.Transparent;
            this._iconFooter.Location = new System.Drawing.Point(10, 10);
            this._iconFooter.Margin = new System.Windows.Forms.Padding(0);
            this._iconFooter.Name = "_iconFooter";
            this._iconFooter.Size = new System.Drawing.Size(16, 16);
            this._iconFooter.TabIndex = 4;
            this._iconFooter.TabStop = false;
            // 
            // _footerLabel
            // 
            this._footerLabel.AutoSize = false;
            this._footerLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._footerLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(57)))), ((int)(((byte)(91)))));
            this._footerLabel.Location = new System.Drawing.Point(36, 11);
            this._footerLabel.Margin = new System.Windows.Forms.Padding(0);
            this._footerLabel.Name = "_footerLabel";
            this._footerLabel.Size = new System.Drawing.Size(78, 15);
            this._footerLabel.Text = "Content";
            // 
            // _panelFooterBorderTop
            // 
            this._panelFooterBorderTop.BorderStyle = Krypton.Toolkit.PaletteBorderStyle.HeaderPrimary;
            this._panelFooterBorderTop.Dock = System.Windows.Forms.DockStyle.Top;
            this._panelFooterBorderTop.Location = new System.Drawing.Point(0, 0);
            this._panelFooterBorderTop.Name = "_panelFooterBorderTop";
            this._panelFooterBorderTop.Size = new System.Drawing.Size(578, 1);
            this._panelFooterBorderTop.Text = "kryptonBorderEdge1";
            // 
            // VisualTaskDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(595, 164);
            this.Controls.Add(this._panelFooter);
            this.Controls.Add(this._panelButtons);
            this.Controls.Add(this._panelMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VisualTaskDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            ((System.ComponentModel.ISupportInitialize)(this._panelMain)).EndInit();
            this._panelMain.ResumeLayout(false);
            this._panelMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._panelMainSpacer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._panelMainCommands)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._panelMainRadio)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._panelMainText)).EndInit();
            this._panelMainText.ResumeLayout(false);
            this._panelMainText.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._panelIcon)).EndInit();
            this._panelIcon.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._messageIcon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._panelButtons)).EndInit();
            this._panelButtons.ResumeLayout(false);
            this._panelButtons.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._panelFooter)).EndInit();
            this._panelFooter.ResumeLayout(false);
            this._panelFooter.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._iconFooter)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion
    }
}
