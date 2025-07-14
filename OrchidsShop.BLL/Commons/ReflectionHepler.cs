using System.Collections;
using System.Reflection;

namespace OrchidsShop.BLL.Commons;

public static class ReflectionHepler
{
/// <summary>
    /// Updates properties from source to destination object
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <param name="source">Source object</param>
    /// <param name="destination">Destination object</param>
    public static void UpdateProperties<TSource, TDestination>(TSource source, TDestination destination)
    {
        var sourceProperties = typeof(TSource).GetProperties();
        foreach (var property in sourceProperties)
        {
            if (property.CanRead && property.Name != "Id") // Skip Id property
            {
                var value = property.GetValue(source);
                if (value != null)
                {
                    var destProperty = typeof(TDestination).GetProperty(property.Name);
                    if (destProperty != null && destProperty.CanWrite)
                    {
                        // Check if the destination property is a collection
                        if (typeof(IEnumerable).IsAssignableFrom(destProperty.PropertyType) && destProperty.PropertyType != typeof(string))
                        {
                            HandleCollectionUpdate(value, destProperty, destination);
                        }
                        // Check if the types are compatible for single value properties
                        else if (destProperty.PropertyType.IsAssignableFrom(value.GetType()))
                        {
                            destProperty.SetValue(destination, value);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Updates properties from source to destination object with excluded properties
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <param name="source">Source object</param>
    /// <param name="destination">Destination object</param>
    /// <param name="excludeProperties">List of property names to exclude</param>
    public static void UpdateProperties<TSource, TDestination>(TSource source, TDestination destination, List<string> excludeProperties = null)
    {
        var sourceProperties = typeof(TSource).GetProperties();
        foreach (var property in sourceProperties)
        {
            if (property.CanRead && (excludeProperties == null || !excludeProperties.Contains(property.Name)))
            {
                if (property.Name != "Id") // Skip Id property
                {
                    var value = property.GetValue(source);
                    if (value != null)
                    {
                        var destProperty = typeof(TDestination).GetProperty(property.Name);
                        if (destProperty != null && destProperty.CanWrite)
                        {
                            // Check if the destination property is a collection
                            if (typeof(IEnumerable).IsAssignableFrom(destProperty.PropertyType) && destProperty.PropertyType != typeof(string))
                            {
                                HandleCollectionUpdate(value, destProperty, destination);
                            }
                            // Check if the types are compatible for single value properties
                            else if (destProperty.PropertyType.IsAssignableFrom(value.GetType()))
                            {
                                destProperty.SetValue(destination, value);
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Handles updating collection properties
    /// </summary>
    /// <param name="sourceValue">Source collection value</param>
    /// <param name="destProperty">Destination property info</param>
    /// <param name="destination">Destination object</param>
    private static void HandleCollectionUpdate(object sourceValue, PropertyInfo destProperty, object destination)
    {
        var sourceCollection = sourceValue as IEnumerable;
        if (sourceCollection == null) return;

        var destCollection = destProperty.GetValue(destination) as IList;
        if (destCollection == null)
        {
            var destCollectionType = destProperty.PropertyType;
            if (destCollectionType.IsInterface)
            {
                // Create a new list if not initialized
                var listType = typeof(List<>).MakeGenericType(destCollectionType.GetGenericArguments().First());
                destCollection = (IList)Activator.CreateInstance(listType);
                destProperty.SetValue(destination, destCollection);
            }
            else
            {
                destCollection = (IList)Activator.CreateInstance(destCollectionType);
                destProperty.SetValue(destination, destCollection);
            }
        }
        else
        {
            destCollection.Clear();
        }

        var elementType = destProperty.PropertyType.GetGenericArguments().First();

        foreach (var sourceItem in sourceCollection)
        {
            if (sourceItem == null) continue;

            if (elementType.IsAssignableFrom(sourceItem.GetType()))
            {
                // If same type, add directly
                destCollection.Add(sourceItem);
            }
            else
            {
                // If complex object type, create new instance and map data
                var destItem = Activator.CreateInstance(elementType);
                UpdateProperties(sourceItem, destItem);
                destCollection.Add(destItem);
            }
        }
    }
}