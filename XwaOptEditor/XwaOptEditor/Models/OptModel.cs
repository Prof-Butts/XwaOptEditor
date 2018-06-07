﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JeremyAnsel.Xwa.Opt;
using JeremyAnsel.Xwa.WpfOpt;
using XwaOptEditor.Mvvm;

namespace XwaOptEditor.Models
{
    public class OptModel : ObservableObject
    {
        public const int UndoStackMaxLength = 20;

        private OptFile file;

        private OptCache cache;

        private bool isPlayable;

        public OptModel()
        {
            this.PlayabilityMessages = new ObservableCollection<PlayabilityMessage>();
            this.UndoStack = new ObservableCollection<Tuple<string, OptFile>>();

            this.file = new OptFile();
            this.UndoStackPush("new");
        }

        public OptFile File
        {
            get
            {
                return this.file;
            }

            set
            {
                this.file = null;
                this.cache = null;
                this.RaisePropertyChangedEvent("File");

                this.file = value;
                this.RaisePropertyChangedEvent("File");

                this.Cache = new OptCache(this.file);

                this.CheckPlayability();
            }
        }

        public OptCache Cache
        {
            get
            {
                return this.cache;
            }

            private set
            {
                this.cache = value;
                this.RaisePropertyChangedEvent("Cache");
            }
        }

        public bool IsPlayable
        {
            get
            {
                return this.isPlayable;
            }

            set
            {
                if (this.isPlayable != value)
                {
                    this.isPlayable = value;
                    this.RaisePropertyChangedEvent("IsPlayable");
                }
            }
        }

        public ObservableCollection<PlayabilityMessage> PlayabilityMessages { get; private set; }

        public ObservableCollection<Tuple<string, OptFile>> UndoStack { get; private set; }

        public void UndoStackPush(string label)
        {
            this.UndoStack.Insert(0, Tuple.Create(label, this.File.Clone()));

            while (this.UndoStack.Count > OptModel.UndoStackMaxLength)
            {
                this.UndoStack.RemoveAt(this.UndoStack.Count - 1);
            }
        }

        public void UndoStackRestore(int index)
        {
            if (index == -1)
            {
                if (this.UndoStack.Count < 1)
                {
                    return;
                }

                index = 0;
            }

            var undo = this.UndoStack[index];
            this.File = undo.Item2;

            this.UndoStack.RemoveAt(index);
            this.UndoStack.Insert(0, Tuple.Create(undo.Item1, undo.Item2.Clone()));
        }

        private void CheckPlayability()
        {
            this.PlayabilityMessages.Clear();
            this.IsPlayable = true;

            if (this.File == null)
            {
                return;
            }

            var messages = this.File.CheckPlayability();

            this.IsPlayable = !messages.Any(t => t.Level == PlayabilityMessageLevel.Error);

            foreach (var message in messages)
            {
                this.PlayabilityMessages.Add(message);
            }
        }
    }
}
