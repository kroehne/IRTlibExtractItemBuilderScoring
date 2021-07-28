using System.Collections.Generic;
using System.Xml.Serialization;

namespace ibxml_8_13_to_9_x.softcon.de
{ 
    [XmlRoot(ElementName = "stimulus")]
    public class Stimulus
    {
        [XmlAttribute(AttributeName = "version")]
        public string Version { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "project")]
        public string Project { get; set; }
        [XmlAttribute(AttributeName = "design-version")]
        public string designVersion { get; set; }
        [XmlAttribute(AttributeName = "ru   ntime-version")]
        public string runtimeVersion { get; set; }
        [XmlAttribute(AttributeName = "itemWidth")]
        public int ItemWidth { get; set; }
        [XmlAttribute(AttributeName = "itemHeight")]
        public int ItemHeight { get; set; }

        [XmlElement(ElementName = "entry-point")]
        public List<EntryPoint> entryPoints { get; set; }
    }

    public class EntryPoint
    {
        [XmlAttribute(AttributeName = "id")]
        public string ID { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "scoringTask")]
        public bool ScoringTask { get; set; }
        [XmlAttribute(AttributeName = "taskToScore")]
        public string TaskToScore { get; set; }
    }

    [XmlRootAttribute(Namespace = "http://cbaitemscore.softcon.de", IsNullable = false)]
    public class Item
    {
        [XmlElement(Namespace = "", ElementName = "itemScoreList")]
        public List<ItemScoreList> ItemScoreList { get; set; }
    }

    public class ItemScoreList
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("minimumHits")]
        public int MinimumHits { get; set; }

        [XmlAttribute("conditionStatus")]
        public bool ConditionStatus { get; set; }

        [XmlAttribute("startPage")]
        public string StartPage { get; set; }

        [XmlAttribute("startXPage")]
        public string StartXPage { get; set; }

        [XmlElement(Namespace = "", ElementName = "hitList")]
        public List<HitMiss> HitList { get; set; }

        [XmlElement(Namespace = "", ElementName = "missList")]
        public List<HitMiss> MissList { get; set; }

        [XmlElement(Namespace = "", ElementName = "classList")]
        public List<ClassList> ClassList { get; set; }

        [XmlAttribute("isScoringTask")]
        public bool IsScoringTask { get; set; }

        [XmlAttribute("taskToScore")]
        public string TaskToScore { get; set; }

        [XmlAttribute("initializationFile")]
        public string InitializationFile { get; set; }
    }

    public class HitMiss
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("weight")]
        public string Weight { get; set; }

        [XmlAttribute("conditionStatus")]
        public bool ConditionStatus { get; set; }

        [XmlAttribute("classProperty")]
        public string ClassProperty { get; set; }

        [XmlAttribute("fileReference")]
        public string FileReference { get; set; }

        [XmlAttribute(Namespace = "http://www.omg.org/XMI", AttributeName = "id")]
        public string Id { get; set; }
    }

    public class ClassList
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("comment")]
        public string Comment { get; set; }

        [XmlAttribute(Namespace = "http://www.omg.org/XMI", AttributeName = "id")]
        public string Id { get; set; }

    }


    [XmlRootAttribute(Namespace = "http://statemachine.softcon.de", IsNullable = false)]
    public class Machine
    {
        [XmlElement(Namespace = "", ElementName = "states")]
        public List<State> States { get; set; }

        [XmlElement(Namespace = "", ElementName = "variables")]
        public List<Variables> Variables { get; set; }

    }

    public class State
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlAttribute("page")]
        public string Page { get; set; }

        [XmlElement(Namespace = "", ElementName = "regions")]
        public List<Regions> Regions { get; set; }

        [XmlElement(Namespace = "", ElementName = "states")]
        public List<State> SubStates { get; set; }
    }

    public class Regions
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement(Namespace = "", ElementName = "substates")]
        public List<State> SubStates { get; set; }
    }

    public class Variables
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

    }

    [XmlRootAttribute(Namespace = "http://www.springframework.org/schema/beans", IsNullable = false)]
    public class beans
    {
        [XmlElement(ElementName = "bean")]
        public List<bean> ListOfBeans { get; set; }
    }

    public class bean
    {
        [XmlAttribute("id")]
        public string idAttribute { get; set; }

        [XmlAttribute("class")]
        public string classAttribute { get; set; }

        [XmlElement(ElementName = "property")]
        public List<property> ListOfProperties { get; set; }
    }

    public class property
    {
        [XmlAttribute("name")]
        public string nameAttribute { get; set; }
        [XmlAttribute("value")]
        public string valueAttribute { get; set; }
        [XmlAttribute("ref")]
        public string refAttribute { get; set; }

        [XmlArray("set")]
        [XmlArrayItem("ref")]
        public List<refClass> ListOfRefs { get; set; }
    }

    [XmlRoot("ref")]
    public class refClass
    {
        [XmlAttribute("bean")]
        public string beanAttribute { get; set; }
    }
}
