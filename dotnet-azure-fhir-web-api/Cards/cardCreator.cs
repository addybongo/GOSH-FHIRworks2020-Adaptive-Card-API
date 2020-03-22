using System;
using Newtonsoft.Json.Linq;
using AdaptiveCards;

namespace HDR_UK_Web_Application.Cards
{
    public class cardCreator
    {
        public AdaptiveTextBlock createTitle(string titleName)
        {
            return new AdaptiveTextBlock()
            {
                Text = titleName,
                Type = "TextBlock",
                HorizontalAlignment = AdaptiveHorizontalAlignment.Center,
                Wrap = true,
                Size = AdaptiveTextSize.Large,
                Weight = AdaptiveTextWeight.Bolder,
                Color = AdaptiveTextColor.Accent
            };
        }

        public AdaptiveContainer createSection(string sectionName, string[] fields, string[] values)
        {
            AdaptiveContainer container = new AdaptiveContainer();
            container.Separator = true;
            
            container.Items.Add(createSectionHeader(sectionName));
            container.Items.Add(createDualTextColumnSection(fields, values));

            return container;
        }
        
        public AdaptiveContainer createSection(string sectionName, string[] values)
        {
            AdaptiveContainer container = new AdaptiveContainer();
            container.Separator = true;

            container.Items.Add(createSectionHeader(sectionName));

            for (int i = 0; i < values.Length; i++)
            {
                container.Items.Add(createTextEntry(values[i]));
            }

            return container;
        }

        public AdaptiveColumnSet createPhotoTextColumnSection(string image_url, uint imageSize)
        {
            AdaptiveColumnSet columnSet = new AdaptiveColumnSet();
            AdaptiveColumn imageColumn = new AdaptiveColumn();
            imageColumn.Items.Add(new AdaptiveImage(image_url)
            {
                PixelWidth = imageSize
            });
            imageColumn.Width = "auto";
            columnSet.Columns.Add(imageColumn);
            columnSet.Columns.Add(new AdaptiveColumn());
            return columnSet;
        }
        
        //Creates a section of 2 columns, one with the field names and the other with field values
        public AdaptiveColumnSet createDualTextColumnSection(string[] fields, string[] values)
        {
            if(fields.Length != values.Length) throw new Exception("The number of fields doesn't match the number of values!");
            
            AdaptiveColumnSet columnSet = new AdaptiveColumnSet();
            
            columnSet.Columns.Add(createColumnOfText(fields, true));
            columnSet.Columns.Add(createColumnOfText(values, false));

            return columnSet;
        }

        private AdaptiveTextBlock createSectionHeader(string sectionName)
        {
            return new AdaptiveTextBlock()
            {
                Type = "TextBlock",
                Text = sectionName,
                Separator = true,
                Spacing = AdaptiveSpacing.Small,
                Weight = AdaptiveTextWeight.Lighter
            };
        }

        public AdaptiveColumn createColumnOfText(string[] values, bool isFieldHeadings)
        {
            AdaptiveColumn column = new AdaptiveColumn()
            {
                Type = "Column",
                Width = isFieldHeadings ? "auto" : "stretch"
            };

            for (int i = 0; i < values.Length; i++)
            {
                AdaptiveTextBlock text = isFieldHeadings ? createTextHeading(values[i]) : createTextEntry(values[i]);
                
                column.Items.Add(text);
            }

            return column;
        }
        
        //Creates a templated blue text object used to show value entries
        private AdaptiveTextBlock createTextHeading(string text)
        {
            return new AdaptiveTextBlock()
            {
                Type = "TextBlock",
                Text = text,
                Spacing = AdaptiveSpacing.None,
                Weight = AdaptiveTextWeight.Bolder
            };
        }
        
        //Creates a templated blue text object used to show value entries
        private AdaptiveTextBlock createTextEntry(string text)
        {
            return new AdaptiveTextBlock()
            {
                Type = "TextBlock",
                Text = text,
                Spacing = AdaptiveSpacing.None,
                Weight = AdaptiveTextWeight.Default,
                Color = AdaptiveTextColor.Accent
            };
        }
    }
}