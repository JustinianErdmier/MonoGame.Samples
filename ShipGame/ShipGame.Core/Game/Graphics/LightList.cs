#region Using Statements

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace ShipGame.Core.Game.Graphics;

public struct Light
{
    public Vector3 position; // position

    public float radius; // radius

    public Vector3 color; // color

    public Light()
    {
        position = Vector3.Zero;
        radius   = 1.0f;
        color    = Vector3.One;
    }

    /// <summary>Create a new list of lights</summary>
    public Light(Vector3 lightPosition, float lightRadius, Vector3 lightColor)
    {
        position = lightPosition;
        radius   = lightRadius;
        color    = lightColor;
    }

    /// <summary>Set light properties to given effect</summary>
    public void SetEffect(EffectParameter effectLightPosition,
                          EffectParameter effectLightColor,
                          Matrix          worldInverse)
    {
        Vector4 positionRadius = new(Vector3.Transform(position, worldInverse), radius);

        if (effectLightPosition != null)
        {
            effectLightPosition.SetValue(positionRadius);
        }

        if (effectLightColor != null)
        {
            effectLightColor.SetValue(color);
        }
    }
}

public class LightList
{
    // ambient light
    public Vector3 ambient = new(x: 0.3f, y: 0.3f, z: 0.3f);

    // list of lights
    public List<Light> lights = new();

    /// <summary>Saves the light list to a xml file</summary>
    public bool Save(string filename)
    {
        // create stream
        Stream stream;
        stream = File.Create(filename);

        if (stream == null)
        {
            return false;
        }

        XDocument document = new();

        XElement amb = new(name: "ambient",
                           new XElement(name: "X", ambient.X),
                           new XElement(name: "Y", ambient.Y),
                           new XElement(name: "Z", ambient.Z));

        document.Add("LightList",
                     amb,
                     new XElement(name: "lights",
                                  () =>
                                  {
                                      List<XElement> contents = new();

                                      foreach (Light e in lights)
                                      {
                                          XElement position = new(name: "position",
                                                                  new XElement(name: "X", e.position.X),
                                                                  new XElement(name: "Y", e.position.Y),
                                                                  new XElement(name: "Z", e.position.Z));

                                          XElement radius = new(name: "radius", e.radius);

                                          XElement color = new(name: "color",
                                                               new XElement(name: "X", e.color.X),
                                                               new XElement(name: "Y", e.color.Y),
                                                               new XElement(name: "Z", e.color.Z));

                                          contents.Add(new XElement(name: "Light", position, radius, color));
                                      }

                                      return contents;
                                  }));

        document.Save(stream);

        // close
        stream.Close();
        stream = null;

        return true;
    }

    /// <summary>Static method to load a light list from a file</summary>
    public static LightList Load(string filename)
    {
        // open file
        Stream stream;

        try
        {
            stream = TitleContainer.OpenStream(filename);
        }
        catch (FileNotFoundException e)
        {
            Console.WriteLine("LightList load error:" + e.Message);
            stream = null;
        }

        if (stream == null)
        {
            return null;
        }

        // load data
        LightList environmentLights = new();
        XDocument document          = XDocument.Load(stream);
        XElement  ambient           = document.Element(name: "LightList").Element(name: "ambient");

        environmentLights.ambient = new Vector3(float.Parse(ambient.Element(name: "X")?.Value ?? "0.3"),
                                                float.Parse(ambient.Element(name: "Y")?.Value ?? "0.3"),
                                                float.Parse(ambient.Element(name: "Z")?.Value ?? "0.3"));

        IEnumerable<XElement> lightElements = document.Descendants(name: "Light");

        IEnumerable<Light> lights = lightElements.Select(lightElement => new Light
        {
            position = new Vector3(float.Parse(lightElement.Element(name: "position")?.Element(name: "X")?.Value ?? "0"),
                                   float.Parse(lightElement.Element(name: "position")?.Element(name: "Y")?.Value ?? "0"),
                                   float.Parse(lightElement.Element(name: "position")?.Element(name: "Z")?.Value ?? "0")),
            radius = float.Parse(lightElement.Element(name: "radius")?.Value ?? "1"),
            color = new Vector3(float.Parse(lightElement.Element(name: "color")?.Element(name: "X")?.Value ?? "0"),
                                float.Parse(lightElement.Element(name: "color")?.Element(name: "Y")?.Value ?? "0"),
                                float.Parse(lightElement.Element(name: "color")?.Element(name: "Z")?.Value ?? "0"))
        });

        foreach (Light light in lights)
        {
            environmentLights.lights.Add(light);
        }


        // close
        stream.Close();
        stream = null;

        return environmentLights;
    }
}
