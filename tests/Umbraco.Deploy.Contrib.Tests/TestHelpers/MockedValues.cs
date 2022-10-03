namespace Umbraco.Deploy.Contrib.Tests.TestHelpers
{
    public class MockedValues
    {
        public static string HtmlValueWith3Images2UniqueAndLocalLink =
            @"<p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed vel molestie risus. Proin eget orci justo. Donec eu viverra lectus. Etiam id pretium nulla, in maximus mi. Sed mauris ipsum, efficitur a pretium a, malesuada quis dui. Morbi congue dapibus elit id dictum. Morbi sodales pulvinar dignissim. Nullam porta tellus felis, at tempus odio molestie ac. Sed blandit turpis ac orci convallis, quis vulputate lorem tristique. In ornare metus id elit posuere, ac ultricies urna tempus. Aliquam sed ligula sit amet orci venenatis consequat eget eget purus. Maecenas sed ante nec elit aliquam mattis. Nam molestie nibh sed erat venenatis tincidunt. Aliquam eget feugiat quam, at sagittis diam. Nulla pretium rhoncus venenatis.</p>
            <p>Proin porta pharetra molestie. Phasellus congue libero sed felis tristique, ac dictum nulla tempor. Proin eleifend laoreet laoreet. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nulla pretium dapibus cursus. Praesent ac sapien tincidunt, condimentum sapien vehicula, cursus magna. Curabitur tellus dui, consequat sit amet quam non, mollis venenatis arcu. Curabitur vitae interdum purus. Sed ac nulla nec neque iaculis hendrerit. Nulla et tortor fringilla, molestie elit in, rhoncus ex. Duis sit amet imperdiet nibh. Fusce maximus ex sed sodales tristique.</p>
            <p><img style=""width: 500px; height: 375px;"" src=""/media/1001/cute-little-kitten-cute-kittens-16288222-1024-768.jpg?width=500&amp;height=375"" alt="""" rel=""1061"" data-id=""1061"" /></p>
            <p><img style=""width: 500px; height: 375px;"" src=""/media/1002/angry-little-kitten-1024-768.jpg?width=500&amp;height=375"" alt="""" rel=""1062"" data-id=""1062"" /></p>
            <p><iframe width=""360"" height=""203"" src=""https://www.youtube.com/embed/kSa-TY4oDjU?feature=oembed"" frameborder=""0"" allowfullscreen=""""></iframe></p><p> </p>
            <p>Here is a <a data-id=""1130"" data-udi=""umb://document/5f31133c38994e46bad8f53be105f71d"" href=""/{localLink:1130}"" title=""Document 1"">link as localLink</a></p>
            <img style=""width: 500px; height: 375px;"" src=""/media/1001/cute-little-kitten-cute-kittens-16288222-1024-768.jpg?width=500&amp;height=375"" alt="""" rel=""1061"" data-id=""1061"" />";

        public static string HtmlValueWith3Images2UniqueAndLocalLinkAsUdis =
            @"<p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed vel molestie risus. Proin eget orci justo. Donec eu viverra lectus. Etiam id pretium nulla, in maximus mi. Sed mauris ipsum, efficitur a pretium a, malesuada quis dui. Morbi congue dapibus elit id dictum. Morbi sodales pulvinar dignissim. Nullam porta tellus felis, at tempus odio molestie ac. Sed blandit turpis ac orci convallis, quis vulputate lorem tristique. In ornare metus id elit posuere, ac ultricies urna tempus. Aliquam sed ligula sit amet orci venenatis consequat eget eget purus. Maecenas sed ante nec elit aliquam mattis. Nam molestie nibh sed erat venenatis tincidunt. Aliquam eget feugiat quam, at sagittis diam. Nulla pretium rhoncus venenatis.</p>
            <p>Proin porta pharetra molestie. Phasellus congue libero sed felis tristique, ac dictum nulla tempor. Proin eleifend laoreet laoreet. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nulla pretium dapibus cursus. Praesent ac sapien tincidunt, condimentum sapien vehicula, cursus magna. Curabitur tellus dui, consequat sit amet quam non, mollis venenatis arcu. Curabitur vitae interdum purus. Sed ac nulla nec neque iaculis hendrerit. Nulla et tortor fringilla, molestie elit in, rhoncus ex. Duis sit amet imperdiet nibh. Fusce maximus ex sed sodales tristique.</p>
            <p><img style=""width: 500px; height: 375px;"" src=""/media/1001/cute-little-kitten-cute-kittens-16288222-1024-768.jpg?width=500&amp;height=375"" alt="""" rel=""1061"" data-id=""1061"" /></p>
            <p><img style=""width: 500px; height: 375px;"" src=""/media/1002/angry-little-kitten-1024-768.jpg?width=500&amp;height=375"" alt="""" rel=""1062"" data-id=""1062"" /></p>
            <p><iframe width=""360"" height=""203"" src=""https://www.youtube.com/embed/kSa-TY4oDjU?feature=oembed"" frameborder=""0"" allowfullscreen=""""></iframe></p><p> </p>
            <p>Here is a <a data-id=""umb://document/5f31133c38994e46bad8f53be105f71d"" data-udi=""umb://document/5f31133c38994e46bad8f53be105f71d"" href=""/{localLink:umb://document/5f31133c38994e46bad8f53be105f71d}"" title=""Document 1"">link as localLink</a></p>
            <img style=""width: 500px; height: 375px;"" src=""/media/1001/cute-little-kitten-cute-kittens-16288222-1024-768.jpg?width=500&amp;height=375"" alt="""" rel=""1061"" data-id=""1061"" />";

        public static string HtmlValueWith3Images2UniqueAndLocalLinkAsUdisConverted =
            @"<p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed vel molestie risus. Proin eget orci justo. Donec eu viverra lectus. Etiam id pretium nulla, in maximus mi. Sed mauris ipsum, efficitur a pretium a, malesuada quis dui. Morbi congue dapibus elit id dictum. Morbi sodales pulvinar dignissim. Nullam porta tellus felis, at tempus odio molestie ac. Sed blandit turpis ac orci convallis, quis vulputate lorem tristique. In ornare metus id elit posuere, ac ultricies urna tempus. Aliquam sed ligula sit amet orci venenatis consequat eget eget purus. Maecenas sed ante nec elit aliquam mattis. Nam molestie nibh sed erat venenatis tincidunt. Aliquam eget feugiat quam, at sagittis diam. Nulla pretium rhoncus venenatis.</p>
            <p>Proin porta pharetra molestie. Phasellus congue libero sed felis tristique, ac dictum nulla tempor. Proin eleifend laoreet laoreet. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nulla pretium dapibus cursus. Praesent ac sapien tincidunt, condimentum sapien vehicula, cursus magna. Curabitur tellus dui, consequat sit amet quam non, mollis venenatis arcu. Curabitur vitae interdum purus. Sed ac nulla nec neque iaculis hendrerit. Nulla et tortor fringilla, molestie elit in, rhoncus ex. Duis sit amet imperdiet nibh. Fusce maximus ex sed sodales tristique.</p>
            <p><img style=""width: 500px; height: 375px;"" src=""/media/1001/cute-little-kitten-cute-kittens-16288222-1024-768.jpg?width=500&amp;height=375"" alt="""" rel=""1061"" data-id=""1061"" /></p>
            <p><img style=""width: 500px; height: 375px;"" src=""/media/1002/angry-little-kitten-1024-768.jpg?width=500&amp;height=375"" alt="""" rel=""1062"" data-id=""1062"" /></p>
            <p><iframe width=""360"" height=""203"" src=""https://www.youtube.com/embed/kSa-TY4oDjU?feature=oembed"" frameborder=""0"" allowfullscreen=""""></iframe></p><p> </p>
            <p>Here is a <a data-id=""1130"" data-udi=""umb://document/5f31133c38994e46bad8f53be105f71d"" href=""/{localLink:umb://document/5f31133c38994e46bad8f53be105f71d}"" title=""Document 1"">link as localLink</a></p>
            <img style=""width: 500px; height: 375px;"" src=""/media/1001/cute-little-kitten-cute-kittens-16288222-1024-768.jpg?width=500&amp;height=375"" alt="""" rel=""1061"" data-id=""1061"" />";

        public static string HtmlValueWith3Images2Unique =
            @"<p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed vel molestie risus. Proin eget orci justo. Donec eu viverra lectus. Etiam id pretium nulla, in maximus mi. Sed mauris ipsum, efficitur a pretium a, malesuada quis dui. Morbi congue dapibus elit id dictum. Morbi sodales pulvinar dignissim. Nullam porta tellus felis, at tempus odio molestie ac. Sed blandit turpis ac orci convallis, quis vulputate lorem tristique. In ornare metus id elit posuere, ac ultricies urna tempus. Aliquam sed ligula sit amet orci venenatis consequat eget eget purus. Maecenas sed ante nec elit aliquam mattis. Nam molestie nibh sed erat venenatis tincidunt. Aliquam eget feugiat quam, at sagittis diam. Nulla pretium rhoncus venenatis.</p>
            <p>Proin porta pharetra molestie. Phasellus congue libero sed felis tristique, ac dictum nulla tempor. Proin eleifend laoreet laoreet. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nulla pretium dapibus cursus. Praesent ac sapien tincidunt, condimentum sapien vehicula, cursus magna. Curabitur tellus dui, consequat sit amet quam non, mollis venenatis arcu. Curabitur vitae interdum purus. Sed ac nulla nec neque iaculis hendrerit. Nulla et tortor fringilla, molestie elit in, rhoncus ex. Duis sit amet imperdiet nibh. Fusce maximus ex sed sodales tristique.</p>
            <p><img style=""width: 500px; height: 375px;"" src=""/media/1001/cute-little-kitten-cute-kittens-16288222-1024-768.jpg?width=500&amp;height=375"" alt="""" rel=""1061"" data-id=""1061"" /></p>
            <p><img style=""width: 500px; height: 375px;"" src=""/media/1002/angry-little-kitten-1024-768.jpg?width=500&amp;height=375"" alt="""" rel=""1062"" data-id=""1062"" /></p>
            <p><iframe width=""360"" height=""203"" src=""https://www.youtube.com/embed/kSa-TY4oDjU?feature=oembed"" frameborder=""0"" allowfullscreen=""""></iframe></p><p> </p>
            <p>Here is a <a data-id=""1130"" href=""/{localLink:1130}"" title=""Document 1"">link as localLink</a></p>
            <img style=""width: 500px; height: 375px;"" src=""/media/1001/cute-little-kitten-cute-kittens-16288222-1024-768.jpg?width=500&amp;height=375"" alt="""" rel=""1061"" data-id=""1061"" />";

        public static string HtmlValueWith3Images2UniqueAsUdis =
            @"<p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed vel molestie risus. Proin eget orci justo. Donec eu viverra lectus. Etiam id pretium nulla, in maximus mi. Sed mauris ipsum, efficitur a pretium a, malesuada quis dui. Morbi congue dapibus elit id dictum. Morbi sodales pulvinar dignissim. Nullam porta tellus felis, at tempus odio molestie ac. Sed blandit turpis ac orci convallis, quis vulputate lorem tristique. In ornare metus id elit posuere, ac ultricies urna tempus. Aliquam sed ligula sit amet orci venenatis consequat eget eget purus. Maecenas sed ante nec elit aliquam mattis. Nam molestie nibh sed erat venenatis tincidunt. Aliquam eget feugiat quam, at sagittis diam. Nulla pretium rhoncus venenatis.</p>
            <p>Proin porta pharetra molestie. Phasellus congue libero sed felis tristique, ac dictum nulla tempor. Proin eleifend laoreet laoreet. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nulla pretium dapibus cursus. Praesent ac sapien tincidunt, condimentum sapien vehicula, cursus magna. Curabitur tellus dui, consequat sit amet quam non, mollis venenatis arcu. Curabitur vitae interdum purus. Sed ac nulla nec neque iaculis hendrerit. Nulla et tortor fringilla, molestie elit in, rhoncus ex. Duis sit amet imperdiet nibh. Fusce maximus ex sed sodales tristique.</p>
            <p><img style=""width: 500px; height: 375px;"" src=""umb://media/eb0ad3395cad417a90051bd871eccc9c?width=500&amp;height=375"" alt="""" rel=""umb://media/eb0ad3395cad417a90051bd871eccc9c"" data-id=""umb://media/eb0ad3395cad417a90051bd871eccc9c"" /></p>
            <p><img style=""width: 500px; height: 375px;"" src=""umb://media/51e50c3a1a494507b4364068c4b429cd?width=500&amp;height=375"" alt="""" rel=""umb://media/51e50c3a1a494507b4364068c4b429cd"" data-id=""umb://media/51e50c3a1a494507b4364068c4b429cd"" /></p>
            <p><iframe width=""360"" height=""203"" src=""https://www.youtube.com/embed/kSa-TY4oDjU?feature=oembed"" frameborder=""0"" allowfullscreen=""""></iframe></p><p> </p>
            <p>Here is a <a data-id=""1130"" href=""/{localLink:1130}"" title=""Document 1"">link as localLink</a></p>
            <img style=""width: 500px; height: 375px;"" src=""umb://media/eb0ad3395cad417a90051bd871eccc9c?width=500&amp;height=375"" alt="""" rel=""umb://media/eb0ad3395cad417a90051bd871eccc9c"" data-id=""umb://media/eb0ad3395cad417a90051bd871eccc9c"" />";

        public static string HtmlValueWith3Images2UniqueAsUdisAndLocalLinkAsUdis =
            @"<p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed vel molestie risus. Proin eget orci justo. Donec eu viverra lectus. Etiam id pretium nulla, in maximus mi. Sed mauris ipsum, efficitur a pretium a, malesuada quis dui. Morbi congue dapibus elit id dictum. Morbi sodales pulvinar dignissim. Nullam porta tellus felis, at tempus odio molestie ac. Sed blandit turpis ac orci convallis, quis vulputate lorem tristique. In ornare metus id elit posuere, ac ultricies urna tempus. Aliquam sed ligula sit amet orci venenatis consequat eget eget purus. Maecenas sed ante nec elit aliquam mattis. Nam molestie nibh sed erat venenatis tincidunt. Aliquam eget feugiat quam, at sagittis diam. Nulla pretium rhoncus venenatis.</p>
            <p>Proin porta pharetra molestie. Phasellus congue libero sed felis tristique, ac dictum nulla tempor. Proin eleifend laoreet laoreet. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nulla pretium dapibus cursus. Praesent ac sapien tincidunt, condimentum sapien vehicula, cursus magna. Curabitur tellus dui, consequat sit amet quam non, mollis venenatis arcu. Curabitur vitae interdum purus. Sed ac nulla nec neque iaculis hendrerit. Nulla et tortor fringilla, molestie elit in, rhoncus ex. Duis sit amet imperdiet nibh. Fusce maximus ex sed sodales tristique.</p>
            <p><img style=""width: 500px; height: 375px;"" src=""umb://media/eb0ad3395cad417a90051bd871eccc9c?width=500&amp;height=375"" alt="""" rel=""umb://media/eb0ad3395cad417a90051bd871eccc9c"" data-id=""umb://media/eb0ad3395cad417a90051bd871eccc9c"" /></p>
            <p><img style=""width: 500px; height: 375px;"" src=""umb://media/51e50c3a1a494507b4364068c4b429cd?width=500&amp;height=375"" alt="""" rel=""umb://media/51e50c3a1a494507b4364068c4b429cd"" data-id=""umb://media/51e50c3a1a494507b4364068c4b429cd"" /></p>
            <p><iframe width=""360"" height=""203"" src=""https://www.youtube.com/embed/kSa-TY4oDjU?feature=oembed"" frameborder=""0"" allowfullscreen=""""></iframe></p><p> </p>
            <p>Here is a <a data-id=""umb://document/5f31133c38994e46bad8f53be105f71d"" href=""/{localLink:umb://document/5f31133c38994e46bad8f53be105f71d}"" title=""Document 1"">link as localLink</a></p>
            <img style=""width: 500px; height: 375px;"" src=""umb://media/eb0ad3395cad417a90051bd871eccc9c?width=500&amp;height=375"" alt="""" rel=""umb://media/eb0ad3395cad417a90051bd871eccc9c"" data-id=""umb://media/eb0ad3395cad417a90051bd871eccc9c"" />";

        public static string HtmlValueWith2LocalLinksOneUdiOneInt =
            @"<p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed vel molestie risus. Proin eget orci justo. Donec eu viverra lectus. Etiam id pretium nulla, in maximus mi. Sed mauris ipsum, efficitur a pretium a, malesuada quis dui. Morbi congue dapibus elit id dictum. Morbi sodales pulvinar dignissim. Nullam porta tellus felis, at tempus odio molestie ac. Sed blandit turpis ac orci convallis, quis vulputate lorem tristique. In ornare metus id elit posuere, ac ultricies urna tempus. Aliquam sed ligula sit amet orci venenatis consequat eget eget purus. Maecenas sed ante nec elit aliquam mattis. Nam molestie nibh sed erat venenatis tincidunt. Aliquam eget feugiat quam, at sagittis diam. Nulla pretium rhoncus venenatis.</p>
            <p>Proin porta pharetra molestie. Phasellus congue libero sed felis tristique, ac dictum nulla tempor. Proin eleifend laoreet laoreet. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nulla pretium dapibus cursus. Praesent ac sapien tincidunt, condimentum sapien vehicula, cursus magna. Curabitur tellus dui, consequat sit amet quam non, mollis venenatis arcu. Curabitur vitae interdum purus. Sed ac nulla nec neque iaculis hendrerit. Nulla et tortor fringilla, molestie elit in, rhoncus ex. Duis sit amet imperdiet nibh. Fusce maximus ex sed sodales tristique.</p>
            <p>Here is a <a data-udi=""umb://document/5f31133c38994e46bad8f53be105f71d"" href=""/{localLink:umb://document/5f31133c38994e46bad8f53be105f71d}"" title=""Document 1"">link as localLink</a></p>
            <p>Here is a <a data-id=""1131"" href=""/{localLink:umb://document/1f31133c38994e46bad8f53be105f71f}"" title=""Document 1"">link as localLink</a></p>";

        public static string HtmlValueWithMacroAndFourParametersOnePicker =
            @"<p>before macro</p>
            <div class=""umb-macro-holder mceNonEditable""><!-- <?UMBRACO_MACRO macroAlias=""MyTestMacro"" param2=""22"" param1=""value1"" param3=""3"" picked=""1170"" /> --> <ins>Macro alias: <strong>MyTestMacro</strong></ins></div>
            <p>after macro</p>";

        public static string HtmlValueWithMacroAndFourParametersOnePickerAsUdi =
            @"<p>before macro</p>
            <div class=""umb-macro-holder mceNonEditable""><!-- <?UMBRACO_MACRO macroAlias=""MyTestMacro"" param2=""22"" param1=""value1"" param3=""3"" picked=""umb://document/5f31133c38994e46bad8f53be105f71d"" /> --> <ins>Macro alias: <strong>MyTestMacro</strong></ins></div>
            <p>after macro</p>";

        public static string HtmlValueWithOldMacroAndFourParametersOnePicker =
            @"<p>before macro</p>
            <umbraco:Macro Alias=""MyTestMacro"" runat=""server"" param2=""22"" param1=""value1"" param3=""3"" picked=""1170"" ></umbraco:Macro>
            <p>after macro</p>";

        public static string HtmlValueWithMacroAndParametersWithAllEditors =
            @"<p>before macro</p>
            <div class=""umb-macro-holder mceNonEditable""><!-- <?UMBRACO_MACRO macroAlias=""CrazyMacro"" contentpicker=""1170"" multiplecontenttypepicker=""[&quot;1051&quot;,&quot;1068&quot;]"" multiplemediaPicker=""1061,1062"" multipletabpicker=""[&quot;tab1&quot;,&quot;tab2&quot;]"" multiPropertyTypePicker=""[&quot;grid&quot;,&quot;rte&quot;]"" tabPicker=""tab2"" propertyTypePicker=""rte"" singleMediaPicker=""1061"" /> --> <ins>Macro alias: <strong>CrazyMacro</strong></ins></div>
            <p>after macro</p>";

        public static string HtmlValueWithMacroAndParametersWithAllEditorsAsUdis =
            @"<p>before macro</p>
            <div class=""umb-macro-holder mceNonEditable""><!-- <?UMBRACO_MACRO macroAlias=""CrazyMacro"" contentpicker=""umb://document/5f31133c38994e46bad8f53be105f71d"" multiplecontenttypepicker=""[&quot;umb://document-type/3f7dbdc823364616b8ec527298fdeab2&quot;,&quot;umb://document-type/d9aee64b048746049a644f3e6a55ae66&quot;]"" multiplemediaPicker=""umb://media/eb0ad3395cad417a90051bd871eccc9c,umb://media/51e50c3a1a494507b4364068c4b429cd"" multipletabpicker=""[&quot;tab1&quot;,&quot;tab2&quot;]"" multiPropertyTypePicker=""[&quot;grid&quot;,&quot;rte&quot;]"" tabPicker=""tab2"" propertyTypePicker=""rte"" singleMediaPicker=""umb://media/eb0ad3395cad417a90051bd871eccc9c"" /> --> <ins>Macro alias: <strong>CrazyMacro</strong></ins></div>
            <p>after macro</p>";

        public static string HtmlValueWithMacroForImageSourceParserTest =
            @"<p>before macro</p>
            <div class=""umb-macro-holder mceNonEditable""><!-- <?UMBRACO_MACRO macroAlias=""MyTestMacro"" param2=""22"" param1=""value1"" param3=""3"" picked=""1061"" /> --> <ins>Macro alias: <strong>MyTestMacro</strong></ins></div>
            <p>after macro</p>";

        public static string HtmlValueWithMacroAsUdiForImageSourceParserTest =
            @"<p>before macro</p>
            <div class=""umb-macro-holder mceNonEditable""><!-- <?UMBRACO_MACRO macroAlias=""MyTestMacro"" param2=""22"" param1=""value1"" param3=""3"" picked=""umb://media/eb0ad3395cad417a90051bd871eccc9c"" /> --> <ins>Macro alias: <strong>MyTestMacro</strong></ins></div>
            <p>after macro</p>";
    }
}
