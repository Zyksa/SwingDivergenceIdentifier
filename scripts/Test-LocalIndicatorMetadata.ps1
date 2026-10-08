[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Path,
    [Parameter(Mandatory)][string]$ExpectedDeveloperId
)

$ErrorActionPreference = 'Stop'
if ([string]::IsNullOrWhiteSpace($ExpectedDeveloperId) -or $ExpectedDeveloperId -eq 'YOUR-DEV-ID') {
    throw 'Un developer ID local valide est nécessaire avant installation.'
}
$inputStream = [IO.File]::OpenRead((Resolve-Path -LiteralPath $Path).Path)
$peReader = [Reflection.PortableExecutable.PEReader]::new($inputStream)
try {
    $metadata = [Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($peReader)
    $ids = @()
    foreach ($handle in $metadata.GetAssemblyDefinition().GetCustomAttributes()) {
        $attribute = $metadata.GetCustomAttribute($handle)
        if ($attribute.Constructor.Kind -ne [Reflection.Metadata.HandleKind]::MemberReference) { continue }
        $constructor = $metadata.GetMemberReference([Reflection.Metadata.MemberReferenceHandle]$attribute.Constructor)
        if ($constructor.Parent.Kind -ne [Reflection.Metadata.HandleKind]::TypeReference) { continue }
        $owner = $metadata.GetTypeReference([Reflection.Metadata.TypeReferenceHandle]$constructor.Parent)
        if ($metadata.GetString($owner.Namespace) -ne 'System.Reflection' -or $metadata.GetString($owner.Name) -ne 'AssemblyMetadataAttribute') { continue }
        $value = $metadata.GetBlobReader($attribute.Value)
        if ($value.ReadUInt16() -ne 1) { continue }
        if ($value.ReadSerializedString() -eq 'DeepchartDevId') { $ids += $value.ReadSerializedString() }
    }
    if ($ids.Count -ne 1 -or $ids[0] -cne $ExpectedDeveloperId) {
        throw "La DLL ne contient pas l'identifiant attendu pour l'installation locale. Aucun fichier installé."
    }
    Write-Output 'Métadonnées locales : ID de la DLL conforme (valeur masquée).'
} finally {
    $peReader.Dispose()
    $inputStream.Dispose()
}
