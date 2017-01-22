﻿using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.DataFlow;
using JetBrains.ProjectModel;
using JetBrains.TextControl;
using JetBrains.TextControl.DocumentMarkup;
using JetBrains.UI.Application;
using JetBrains.UI.Controls.GotoByName;
using JetBrains.UI.GotoByName;
using JetBrains.Util;

namespace LiveTemplateShortcuts.GotoWordSample
{
    [ShellComponent]
    public sealed class GotoWordControllerFactory
    {
        [NotNull]
        private readonly IDocumentMarkupManager myMarkupManager;

        [NotNull]
        private readonly GotoByNameMenuComponent myMenuComponent;

        [NotNull]
        private readonly IShellLocks myShellLocks;

        [NotNull]
        private readonly UIApplication myUiApplication;

        public GotoWordControllerFactory([NotNull] IShellLocks shellLocks, [NotNull] UIApplication uiApplication, [NotNull] GotoByNameMenuComponent menuComponent,
                                         [NotNull] IDocumentMarkupManager markupManager)
        {
            myShellLocks = shellLocks;
            myUiApplication = uiApplication;
            myMenuComponent = menuComponent;
            myMarkupManager = markupManager;
        }

        public void ShowMenu([NotNull] IProjectModelElement projectElement, [CanBeNull] ITextControl textControl, [CanBeNull] GotoByNameDataConstants.SearchTextData initialText)
        {
            var solution = projectElement.GetSolution();
            var definition = Lifetimes.Define(solution.GetLifetime());

            var controller = new GotoWordController(definition.Lifetime, myShellLocks, projectElement, textControl, myMarkupManager);

            if (textControl != null)
            {
                // use selected text if there is no initial
                // todo: how to make this work with recent list?
                var selection = textControl.Selection.Ranges.Value;
                if (selection != null &&
                    selection.Count == 1)
                {
                    var docRange = selection[0].ToDocRangeNormalized();
                    if (docRange.Length > 0)
                    {
                        var selectedText = textControl.Document.GetText(docRange);
                        initialText = new GotoByNameDataConstants.SearchTextData(selectedText, TextRange.FromLength(selectedText.Length));
                    }
                }
            }

            var menu = new GotoByNameMenu(myMenuComponent, definition, controller.Model, myUiApplication.MainWindow.GetPrimaryWindow(), initialText);

            var menuDoc = menu.MenuView.Value.Document.NotNull("menuDoc != null");
            menuDoc.SelectedItem.FlowInto(definition.Lifetime, controller.SelectedItem, item => item != null ? item.Key : null);
        }
    }
}