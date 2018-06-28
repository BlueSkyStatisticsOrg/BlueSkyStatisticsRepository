using System.Windows.Controls;
using System.Windows;

namespace BSky.Controls.DesignerSupport
{
   

    public class ManageSubstitution : PropertyEditorBase
    {
        private static int count = 0;
        public ManageSubstitution()
        {
        }
        protected override Control GetEditControl(string PropName, object CurrentValue, object CurrentObj)
        {

            ObjectWrapper placeHolder = CurrentObj as ObjectWrapper;
            // DragDropList selectedElement = placeHolder.SelectedObject as DragDropList;
            if (placeHolder.SelectedObject is BSkyGroupingVariable)
            {
                BSkyGroupingVariable draglist = placeHolder.SelectedObject as BSkyGroupingVariable;
                SubstitutionSettings w = new SubstitutionSettings(draglist.PrefixTxt, draglist.SepCharacter);
                w.SubstituteSettings = CurrentValue.ToString();
                return w;
            }
            if (placeHolder.SelectedObject is BSkyAggregateCtrl)
            {
                BSkyAggregateCtrl agglist = placeHolder.SelectedObject as BSkyAggregateCtrl;
                SubstitutionSettings w = new SubstitutionSettings(agglist.PrefixTxt, agglist.SepCharacter);
                w.SubstituteSettings = CurrentValue.ToString();
                return w;

            }

            if (placeHolder.SelectedObject is BSkySortCtrl)
            {
                BSkySortCtrl sortlist = placeHolder.SelectedObject as BSkySortCtrl;
                SubstitutionSettings w = new SubstitutionSettings(sortlist.PrefixTxt, sortlist.SepCharacter);
                w.SubstituteSettings = CurrentValue.ToString();
                return w;

            }


            if (placeHolder.SelectedObject is DragDropList)
            {
                DragDropList draglist = placeHolder.SelectedObject as DragDropList;
                SubstitutionSettings w = new SubstitutionSettings(draglist.PrefixTxt, draglist.SepCharacter);
                w.SubstituteSettings = CurrentValue.ToString();
                return w;
            }
            else if (placeHolder.SelectedObject is BSkyListBoxwBorderForDatasets)
            {
                BSkyListBoxwBorderForDatasets draglist = placeHolder.SelectedObject as BSkyListBoxwBorderForDatasets;
                SubsSettingsForDatasets w = new SubsSettingsForDatasets(draglist.PrefixTxt, draglist.SepCharacter);
                w.SubstituteSettings = CurrentValue.ToString();
                return w;
            }
            return null;

        }

        protected override object GetEditedValue(Control EditControl, string PropertyName, object oldValue, object currentObj)
        {

            ObjectWrapper placeHolder = currentObj as ObjectWrapper;

            // DragDropList selectedElement = placeHolder.SelectedObject as DragDropList;

            if (placeHolder.SelectedObject is DragDropList)
            {
                DragDropList draglist = placeHolder.SelectedObject as DragDropList;
                if (EditControl is SubstitutionSettings)
                {
                    SubstitutionSettings w = EditControl as SubstitutionSettings;
                    draglist.PrefixTxt = w.PrefixString.Text;
                    draglist.SepCharacter = w.SepCharacter.Text;
                    FrameworkElement selectedElement = currentObj as FrameworkElement;
                    if (w.DialogResult.HasValue && w.DialogResult.Value)
                    {
                        return w.SubstituteSettings;
                    }
                    return oldValue;
                }
                return false;
            }
            else if (placeHolder.SelectedObject is BSkyListBoxwBorderForDatasets)
            {
                BSkyListBoxwBorderForDatasets draglist = placeHolder.SelectedObject as BSkyListBoxwBorderForDatasets;
                if (EditControl is SubsSettingsForDatasets)
                {
                    SubsSettingsForDatasets w = EditControl as SubsSettingsForDatasets;
                    draglist.PrefixTxt = w.PrefixString.Text;
                    draglist.SepCharacter = w.SepCharacter.Text;
                    FrameworkElement selectedElement = currentObj as FrameworkElement;
                    if (w.DialogResult.HasValue && w.DialogResult.Value)
                    {
                        return w.SubstituteSettings;
                    }
                    return oldValue;
                }
                return false;
            }
            if (placeHolder.SelectedObject is BSkyGroupingVariable)
            {
                BSkyGroupingVariable draglist = placeHolder.SelectedObject as BSkyGroupingVariable;
                if (EditControl is SubstitutionSettings)
                {
                    SubstitutionSettings w = EditControl as SubstitutionSettings;
                    draglist.PrefixTxt = w.PrefixString.Text;
                    draglist.SepCharacter = w.SepCharacter.Text;
                    FrameworkElement selectedElement = currentObj as FrameworkElement;
                    if (w.DialogResult.HasValue && w.DialogResult.Value)
                    {
                        return w.SubstituteSettings;
                    }
                    return oldValue;
                }
                return false;
            }
            if (placeHolder.SelectedObject is BSkyAggregateCtrl)
            {
                BSkyAggregateCtrl agglist = placeHolder.SelectedObject as BSkyAggregateCtrl;
                if (EditControl is SubstitutionSettings)
                {
                    SubstitutionSettings w = EditControl as SubstitutionSettings;
                    agglist.PrefixTxt = w.PrefixString.Text;
                    agglist.SepCharacter = w.SepCharacter.Text;
                    FrameworkElement selectedElement = currentObj as FrameworkElement;
                    if (w.DialogResult.HasValue && w.DialogResult.Value)
                    {
                        return w.SubstituteSettings;
                    }
                    return oldValue;
                }
                return false;
            }

            if (placeHolder.SelectedObject is BSkySortCtrl)
            {
                BSkySortCtrl sortlist = placeHolder.SelectedObject as BSkySortCtrl;
                if (EditControl is SubstitutionSettings)
                {
                    SubstitutionSettings w = EditControl as SubstitutionSettings;
                    sortlist.PrefixTxt = w.PrefixString.Text;
                    sortlist.SepCharacter = w.SepCharacter.Text;
                    FrameworkElement selectedElement = currentObj as FrameworkElement;
                    if (w.DialogResult.HasValue && w.DialogResult.Value)
                    {
                        return w.SubstituteSettings;
                    }
                    return oldValue;
                }
                return false;
            }


            return false;
        }


    }
}
