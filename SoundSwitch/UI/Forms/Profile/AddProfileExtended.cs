﻿using System;
using System.Linq;
using System.Windows.Forms;
using SoundSwitch.Framework.Profile;
using SoundSwitch.Framework.Profile.Trigger;
using SoundSwitch.Localization;
using SoundSwitch.Properties;
using SoundSwitch.UI.Component;

namespace SoundSwitch.UI.Forms.Profile
{
    public partial class AddProfileExtended : Form
    {
        private readonly Framework.Profile.Profile _profile;
        private readonly TriggerFactory _triggerFactory;

        public AddProfileExtended(Framework.Profile.Profile profile)
        {
            _profile = profile;
            _triggerFactory = new TriggerFactory();
            InitializeComponent();

            textInput.Hide();
            hotKeyControl.Hide();
            descriptionLabel.Hide();

            LocalizeForm();
            Icon = Resources.profile;
            InitializeFromProfile();
        }

        private void LocalizeForm()
        {
            descriptionBox.Text = SettingsStrings.profile_desc;
            availableTriggersText.Text = SettingsStrings.profile_trigger_available;
            activeTriggerLabel.Text = SettingsStrings.profile_trigger_actives;
        }

        private void InitializeFromProfile()
        {
            InitializeAvailableTriggers();
            setTriggerBox.Items.AddRange(_profile.Triggers.Cast<object>().ToArray());
        }

        private void InitializeAvailableTriggers()
        {
            var countByTrigger = _profile.Triggers.GroupBy(trigger => trigger.Type)
                .ToDictionary(triggers => triggers.Key);
            var availableTriggers = _triggerFactory.AllImplementations
                .Where(pair =>
                {
                    if (countByTrigger.TryGetValue(pair.Key, out var trigger))
                    {
                        return pair.Value.MaxOccurence == -1 || trigger.Count() < pair.Value.MaxOccurence;
                    }

                    return true;
                })
                .Select(pair => pair.Value)
                .Cast<object>()
                .ToArray();

            if (availableTriggerBox.Items.Count != availableTriggers.Length)
            {
                availableTriggerBox.Items.Clear();
                availableTriggerBox.Items.AddRange(availableTriggers);
                availableTriggerBox.SelectedIndex = 0;
            }
        }

        private void addTriggerButton_Click(object sender, EventArgs e)
        {
            if (availableTriggerBox.SelectedItem == null)
            {
                return;
            }

            var trigger = new Trigger(((ITriggerDefinition) availableTriggerBox.SelectedItem).TypeEnum);
            setTriggerBox.Items.Add(trigger);
            setTriggerBox.SelectedItem = trigger;
            _profile.Triggers.Add(trigger);
            InitializeAvailableTriggers();
        }

        private void setTriggerBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (setTriggerBox.SelectedItem == null)
            {
                return;
            }

            var trigger = (Trigger) setTriggerBox.SelectedItem;
            descriptionLabel.Text = _triggerFactory.Get(trigger.Type).Description;
            descriptionLabel.Show();
            switch (trigger.Type)
            {
                case TriggerFactory.Enum.HotKey:
                    textInput.Hide();
                    hotKeyControl.HotKey = trigger.HotKey;
                    hotKeyControl.CleanHotKeyChangedHandler();
                    hotKeyControl.HotKeyChanged += (o, @event) => trigger.HotKey = hotKeyControl.HotKey;
                    hotKeyControl.Show();
                    break;
                case TriggerFactory.Enum.Window:
                    hotKeyControl.Hide();
                    break;
                case TriggerFactory.Enum.Process:
                    hotKeyControl.Hide();
                    break;
                case TriggerFactory.Enum.Steam:
                    textInput.Hide();
                    hotKeyControl.Hide();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (setTriggerBox.SelectedItem == null)
            {
                return;
            }

            //Remove first from the profile, else the SelectedItem will be null
            var trigger = (Trigger) setTriggerBox.SelectedItem;
            _profile.Triggers.Remove(trigger);
            setTriggerBox.Items.Remove(trigger);
            InitializeAvailableTriggers();
            if (setTriggerBox.Items.Count > 0)
            {
                setTriggerBox.SelectedIndex = 0;
            }
        }
    }
}