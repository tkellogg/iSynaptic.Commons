﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSynaptic.Commons.Data.MetadataDeclarations
{
    public class StringMetadataDeclaration : MetadataDeclaration<string>
    {
        public StringMetadataDeclaration() : this(0, int.MaxValue, null)
        {
        }

        public StringMetadataDeclaration(int minLength, int maxLength, string @default) : this(minLength, maxLength, @default, true, true, true)
        {
        }

        public StringMetadataDeclaration(int minLength, int maxLength, string @default, bool isNullPermitted, bool isEmptyPermitted, bool isWhitespacePermitted)
            : base(@default)
        {
            MinLength = minLength;
            MaxLength = maxLength;

            IsNullPermitted = isNullPermitted;
            IsEmptyPermitted = isEmptyPermitted;
            IsWhitespaceOnlyPermitted = isWhitespacePermitted;
        }

        protected override void OnValidateValue(string value, string valueName)
        {
            if(value == null)
            {
                if(IsNullPermitted != true)
                    throw new MetadataValidationException<string>(this, value, string.Format("The {0} value must not be null.", valueName));

                return;
            }

            if (value == string.Empty && IsEmptyPermitted != true)
                throw new MetadataValidationException<string>(this, value, string.Format("The {0} value must not be empty.", valueName));

            if(string.IsNullOrWhiteSpace(value) && IsWhitespaceOnlyPermitted)
                throw new MetadataValidationException<string>(this, value, string.Format("The {0} value must not be only whitespace.", valueName));

            if (valueName.Length < MinLength)
                throw new MetadataValidationException<string>(this, value, string.Format("The {0} value must be at least {1} characters long.", valueName, MinLength));

            if (valueName.Length > MaxLength)
                throw new MetadataValidationException<string>(this, value, string.Format("The {0} value must be no more than {1} characters long.", valueName, MaxLength));
            
            base.OnValidateValue(value, valueName);
        }

        public int MinLength { get; private set; }
        public int MaxLength { get; private set; }

        public bool IsNullPermitted { get; private set; }
        public bool IsEmptyPermitted { get; private set; }
        public bool IsWhitespaceOnlyPermitted { get; private set; }
    }
}
