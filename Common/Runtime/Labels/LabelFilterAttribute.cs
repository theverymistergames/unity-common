﻿using System;
using UnityEngine;

namespace MisterGames.Common.Labels {
    
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class LabelFilterAttribute : PropertyAttribute {
        
        public readonly string path;

        public LabelFilterAttribute(string path) {
            this.path = path;
        }
    }
    
}