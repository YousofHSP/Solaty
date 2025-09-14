using AutoMapper;
using Common.Utilities;
using Domain.Entities;

namespace Shared.DTOs;

public class ImportedRecordFilterDto
{
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 10;
    public ImportedFileType? Type { get; set; }
    public ImportedRecordStatus? Status { get; set; }
    public string? Date { get; set; } = "";
    public string? Search { get; set; } = "";
}

public class AdaptImportedRecordDto
{
    public Dictionary<string, int> Data { get; set; }
    public Dictionary<string, string>? Items{ get; set; }
}

public class ImportedRecordResDto : BaseDto<ImportedRecordResDto, ImportedRecord>
{
    public string Title { get; set; }
    public string TypeTitle { get; set; }
    public ImportedFileType Type { get; set; }
    public string StatusTitle { get; set; }
    public string CreateDate { get; set; }
    public string FileName { get; set; }
    public string Data { get; set; }

    protected override void CustomMappings(IMappingExpression<ImportedRecord, ImportedRecordResDto> mapping)
    {
        mapping.ForMember(
            d => d.TypeTitle,
            s => s.MapFrom(m => m.ImportedFile.Type.ToDisplay(EnumExtensions.DisplayProperty.Name)));
        mapping.ForMember(
            d => d.Type,
            s => s.MapFrom(m => m.ImportedFile.Type));
        mapping.ForMember(
            d => d.FileName,
            s => s.MapFrom(m => m.ImportedFile.Title));
        mapping.ForMember(
            d => d.StatusTitle,
            s => s.MapFrom(m => m.Status.ToDisplay(EnumExtensions.DisplayProperty.Name)));
        mapping.ForMember(
            d => d.CreateDate,
            s => s.MapFrom(m => m.CreateDate.ToShamsi(true)));
        mapping.AfterMap((src, dest) =>
        {
            dest.Data = "";
        });
    }
}