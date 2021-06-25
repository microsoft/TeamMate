using Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Linq;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.IO.Packaging
{
    /// <summary>
    /// Provides extension methods to the System.IO.Packaing.Package type
    /// designed to make writing both files and steams into packages.
    /// </summary>
    public static class PackageExtensions
    {
        /// <summary>
        /// Create a package part in the package.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <param name="partUri">Destination Uri of the file in the package.</param>
        /// <param name="contentType">Media type of the file.</param>
        /// <param name="relationshipType">The relationship of the package part to its parent.</param>
        /// <returns>A package part.</returns>
        public static PackagePart CreateRelatedPart(this Package package, Uri partUri, string contentType, string relationshipType)
        {
            Assert.ParamIsNotNull(package, "package");
            Assert.ParamIsNotNull(partUri, "partUri");
            Assert.ParamIsNotNull(contentType, "contentType");
            Assert.ParamIsNotNull(relationshipType, "relationshipType");

            partUri = PackUriHelper.CreatePartUri(partUri);
            PackagePart part = package.CreatePart(partUri, contentType);
            package.CreateRelationship(part.Uri, TargetMode.Internal, relationshipType);

            return part;
        }

        /// <summary>
        /// Creates a related part.
        /// </summary>
        /// <param name="relatedPart">The related part.</param>
        /// <param name="partUri">The part URI.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="relationshipType">Type of the relationship.</param>
        /// <returns>A package part.</returns>
        public static PackagePart CreateRelatedPart(this PackagePart relatedPart, Uri partUri, string contentType, string relationshipType)
        {
            Assert.ParamIsNotNull(relatedPart, "relatedPart");
            Assert.ParamIsNotNull(partUri, "partUri");
            Assert.ParamIsNotNull(contentType, "contentType");
            Assert.ParamIsNotNull(relationshipType, "relationshipType");

            partUri = PackUriHelper.CreatePartUri(partUri);
            PackagePart part = relatedPart.Package.CreatePart(partUri, contentType);
            relatedPart.CreateRelationship(part.Uri, TargetMode.Internal, relationshipType);
            return part;
        }

        /// <summary>
        /// Gets an existing package part (based on the part uri), or creates it if it doesn't already exist, including a relationship for the part.
        /// </summary>
        /// <param name="package">A package.</param>
        /// <param name="partUri">The target part URI.</param>
        /// <param name="contentType">The media type describing the part (used for creation).</param>
        /// <param name="relationshipType">The relationship type (used for creating the relationship).</param>
        /// <returns>The looked up or created part.</returns>
        public static PackagePart GetOrCreateRelatedPart(this Package package, Uri partUri, string contentType, string relationshipType)
        {
            Assert.ParamIsNotNull(package, "package");

            return DoGetOrCreatePart(package, partUri, contentType, relationshipType, null);
        }

        /// <summary>
        /// Gets an existing package part (based on the part uri), or creates it if it doesn't already exist, including a relationship for the part.
        /// </summary>
        /// <param name="parentPart">A parent part (for creating the relationship).</param>
        /// <param name="partUri">The target part URI.</param>
        /// <param name="contentType">The media type describing the part (used for creation).</param>
        /// <param name="relationshipType">The relationship type (used for creating the relationship).</param>
        /// <returns>The looked up or created part.</returns>
        public static PackagePart GetOrCreateRelatedPart(this PackagePart parentPart, Uri partUri, string contentType, string relationshipType)
        {
            Assert.ParamIsNotNull(parentPart, "parentPart");

            return DoGetOrCreatePart(parentPart.Package, partUri, contentType, relationshipType, parentPart);
        }

        /// <summary>
        /// Gets an existing package part (based on the part uri), or creates it if it doesn't already exist, including a relationship for the part.
        /// </summary>
        /// <param name="package">A package.</param>
        /// <param name="partUri">The target part URI.</param>
        /// <param name="contentType">The media type describing the part (used for creation).</param>
        /// <param name="relationshipType">The relationship type (used for creating the relationship).</param>
        /// <param name="relatedPart">An optional parent part (for creating the relationship), or <c>null</c> to add a global relationship with the package.</param>
        /// <returns>The looked up or created part.</returns>
        private static PackagePart DoGetOrCreatePart(Package package, Uri partUri, string contentType, string relationshipType, PackagePart relatedPart)
        {
            Assert.ParamIsNotNull(package, "package");
            Assert.ParamIsNotNull(partUri, "partUri");
            Assert.ParamIsNotNull(contentType, "contentType");
            Assert.ParamIsNotNull(relationshipType, "relationshipType");

            PackagePart result = package.TryGetPart(partUri);
            if (result != null)
            {
                // Found an existing part, at least assert that the content-type is what we expected
                Debug.Assert(String.Equals(contentType, result.ContentType), String.Format(CultureInfo.CurrentCulture,
                    "Unexpected content type for part {0}. Expected {1}, actual was {2}", partUri, contentType, result.ContentType));
            }
            else if(relatedPart != null)
            {
                result = relatedPart.CreateRelatedPart(partUri, contentType, relationshipType);
            }
            else if (relatedPart != null)
            {
                result = package.CreateRelatedPart(partUri, contentType, relationshipType);
            }

            return result;
        }

        /// <summary>
        /// Gets a writeable stream for a package part (overwriting the part contents if it already exists).
        /// </summary>
        /// <param name="part">A part.</param>
        /// <returns>A writeable stream for the part.</returns>
        public static Stream OpenWrite(this PackagePart part)
        {
            Assert.ParamIsNotNull(part, "part");

            return part.GetStream(FileMode.Create, FileAccess.Write);
        }

        /// <summary>
        /// Opens a read-only stream for a package part..
        /// </summary>
        /// <param name="part">The part.</param>
        /// <returns>The opened stream for reading.</returns>
        public static Stream OpenRead(this PackagePart part)
        {
            Assert.ParamIsNotNull(part, "part");

            return part.GetStream(FileMode.Open, FileAccess.Read);
        }

        /// <summary>
        /// Extracts the contents of a package part to an output file.
        /// </summary>
        /// <param name="part">A part.</param>
        /// <param name="outputFile">The output file.</param>
        public static void ExtractTo(this PackagePart part, string outputFile)
        {
            Assert.ParamIsNotNull(outputFile, "outputFile");

            using (Stream sourceStream = part.GetStream())
            using (Stream outputStream = File.Create(outputFile))
            {
                sourceStream.CopyTo(outputStream);
            }
        }

        /// <summary>
        /// Deletes a package part and all of its relationships and owned parts in a cascading
        /// (recursive) fashion.
        /// </summary>
        /// <param name="rootPart">The root package part to remove.</param>
        public static void CascadeDelete(this PackagePart rootPart)
        {
            Assert.ParamIsNotNull(rootPart, "rootPart");

            // Do a breadth first traversal of relationships, avoiding circular loops
            Package package = rootPart.Package;
            ICollection<PackagePart> partsToRemove = new HashSet<PackagePart>();
            Queue<PackagePart> partsToProcess = new Queue<PackagePart>();
            partsToProcess.Enqueue(rootPart);

            while (partsToProcess.Any())
            {
                PackagePart part = partsToProcess.Dequeue();
                partsToRemove.Add(part);

                foreach (PackageRelationship relationship in part.GetRelationships())
                {
                    PackagePart targetPart = relationship.TryGetInternalPart();
                    if (targetPart != null && !partsToRemove.Contains(targetPart))
                    {
                        partsToProcess.Enqueue(targetPart);
                    }
                }
            }

            foreach (PackagePart part in partsToRemove)
            {
                package.DeletePart(part.Uri);
            }
        }

        /// <summary>
        /// Deletes a relationship and all of its target parts (and relationships) in a cascading
        /// (recursive) fashion.
        /// </summary>
        /// <param name="relationship">A package relationship.</param>
        public static void CascadeDeleteRelationship(this Package package, PackageRelationship relationship)
        {
            Assert.ParamIsNotNull(package, "package");
            Assert.ParamIsNotNull(relationship, "relationship");

            CascadeDelete(relationship);
            package.DeleteRelationship(relationship.Id);
        }

        /// <summary>
        /// Deletes a relationship and all of its target parts (and relationships) in a cascading
        /// (recursive) fashion.
        /// </summary>
        /// <param name="relationship">A package relationship.</param>
        public static void CascadeDeleteRelationship(this PackagePart part, PackageRelationship relationship)
        {
            Assert.ParamIsNotNull(part, "part");
            Assert.ParamIsNotNull(relationship, "relationship");

            CascadeDelete(relationship);
            part.DeleteRelationship(relationship.Id);
        }

        /// <summary>
        /// Deletes all of the related parts of a relationship in a cascading (recursive) fashion.
        /// </summary>
        /// <param name="relationship"></param>
        private static void CascadeDelete(PackageRelationship relationship)
        {
            PackagePart part = relationship.TryGetInternalPart();
            if (part != null)
            {
                part.CascadeDelete();
            }
        }

        /// <summary>
        /// Gets a single relationship by type.
        /// </summary>
        /// <param name="package">A package.</param>
        /// <param name="relationshipType">The relationship type.</param>
        /// <returns>The single relationship of that type that was found, or <c>null</c> if none were found.</returns>
        public static PackageRelationship GetSingleRelationshipByType(this Package package, string relationshipType)
        {
            Assert.ParamIsNotNull(package, "package");
            Assert.ParamIsNotNull(relationshipType, "relationshipType");

            return GetFirstRelationship(package.GetRelationshipsByType(relationshipType), relationshipType);
        }

        /// <summary>
        /// Gets a single relationship by type.
        /// </summary>
        /// <param name="part">A package part.</param>
        /// <param name="relationshipType">The relationship type.</param>
        /// <returns>The single relationship of that type that was found, or <c>null</c> if none were found.</returns>
        public static PackageRelationship GetSingleRelationshipByType(this PackagePart part, string relationshipType)
        {
            Assert.ParamIsNotNull(part, "part");
            Assert.ParamIsNotNull(relationshipType, "relationshipType");

            return GetFirstRelationship(part.GetRelationshipsByType(relationshipType), relationshipType);
        }

        /// <summary>
        /// Gets a single relationship by matching the target URI.
        /// </summary>
        /// <param name="package">A package.</param>
        /// <param name="targetUri">The target URI.</param>
        /// <returns>The matching relationship, or <c>null</c> if none found.</returns>
        public static PackageRelationship GetSingleRelationshipByTargetUri(this Package package, Uri targetUri)
        {
            Assert.ParamIsNotNull(package, "package");
            Assert.ParamIsNotNull(targetUri, "targetUri");

            return package.GetRelationships().FirstOrDefault(rel => rel.TargetUri.Equals(targetUri));
        }

        /// <summary>
        /// Gets a single relationship by matching the target URI.
        /// </summary>
        /// <param name="part">A package part.</param>
        /// <param name="targetUri">The target URI.</param>
        /// <returns>The matching relationship, or <c>null</c> if none found.</returns>
        public static PackageRelationship GetSingleRelationshipByTargetUri(this PackagePart part, Uri targetUri)
        {
            Assert.ParamIsNotNull(part, "part");
            Assert.ParamIsNotNull(targetUri, "targetUri");

            return part.GetRelationships().FirstOrDefault(rel => rel.TargetUri.Equals(targetUri));
        }

        /// <summary>
        /// Gets a single part associated through a given relationship type.
        /// </summary>
        /// <param name="package">A package.</param>
        /// <param name="relationshipType">The relationship type.</param>
        /// <returns>The single related part, or <c>null</c> if not found.</returns>
        public static PackagePart GetSingleRelatedPart(this Package package, string relationshipType)
        {
            Assert.ParamIsNotNull(package, "package");
            Assert.ParamIsNotNull(relationshipType, "relationshipType");

            PackageRelationship relationship = package.GetSingleRelationshipByType(relationshipType);
            return (relationship != null) ? relationship.TryGetInternalPart() : null;
        }

        /// <summary>
        /// Gets a single part associated through a given relationship type.
        /// </summary>
        /// <param name="part">A package part.</param>
        /// <param name="relationshipType">The relationship type.</param>
        /// <returns>The single related part, or <c>null</c> if not found.</returns>
        public static PackagePart GetSingleRelatedPart(this PackagePart part, string relationshipType)
        {
            Assert.ParamIsNotNull(part, "part");
            Assert.ParamIsNotNull(relationshipType, "relationshipType");

            PackageRelationship relationship = part.GetSingleRelationshipByType(relationshipType);
            return (relationship != null) ? relationship.TryGetInternalPart() : null;
        }

        /// <summary>
        /// Gets the package part of a relationship, if it is internal and it exists.
        /// </summary>
        /// <param name="relationship">A relationship.</param>
        /// <returns>The related part, or <c>null</c> if not internal or doesn't exist.</returns>
        public static PackagePart TryGetInternalPart(this PackageRelationship relationship)
        {
            Assert.ParamIsNotNull(relationship, "relationship");

            PackagePart part = null;

            if (relationship.TargetMode == TargetMode.Internal)
            {
                var partUri = PackUriHelper.ResolvePartUri(relationship.SourceUri, relationship.TargetUri);
                part = relationship.Package.TryGetPart(partUri);
            }
            else
            {
                string message = "Requested part that is not internal for relationship: " + relationship;
                Debug.Fail(message);
            }

            return part;
        }

        /// <summary>
        /// Tries to get a package part, if the uri exists. This is different from GetPart() in that that will
        /// throw if the URI doesn't exist.
        /// </summary>
        /// <param name="package">A package.</param>
        /// <param name="partUri">A part uri.</param>
        /// <returns>The package part, or <c>null</c> if the URI doesn't exist.</returns>
        public static PackagePart TryGetPart(this Package package, Uri partUri)
        {
            Assert.ParamIsNotNull(package, "package");
            Assert.ParamIsNotNull(partUri, "partUri");

            PackagePart part = null;

            if (package.PartExists(partUri))
            {
                part = package.GetPart(partUri);
            }

            return part;
        }

        /// <summary>
        /// Deletes a package part from a given package.
        /// </summary>
        /// <param name="part">The part.</param>
        public static void Delete(this PackagePart part)
        {
            part.Package.DeletePart(part.Uri);
        }

        /// <summary>
        /// Gets the first relationship in a collection of relationships.
        /// </summary>
        /// <param name="relationships">A collection of relationships.</param>
        /// <param name="relationshipType">The relationship type that was used to query these relationships.</param>
        /// <returns>The first relationship (if available) or <c>null</c> if none are available.</returns>
        private static PackageRelationship GetFirstRelationship(PackageRelationshipCollection relationships, string relationshipType)
        {
            int relationShipCount = relationships.Count();
            if (relationShipCount > 0)
            {
                if (relationShipCount != 1)
                {
                    string message = String.Format(CultureInfo.CurrentCulture, "Expected single relationship of type {0}. Instead, we found {1}", relationshipType, relationShipCount);
                    Debug.Fail(message);
                }

                return relationships.First();
            }

            return null;
        }
    }
}
