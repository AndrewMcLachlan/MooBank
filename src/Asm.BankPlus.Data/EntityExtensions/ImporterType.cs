namespace Asm.MooBank.Data.Entities
{
    public partial class ImporterType
    {
        public static explicit operator Models.ImporterType(ImporterType entity)
        {
            return new Models.ImporterType
            {
                Id = entity.ImporterTypeId,
                Type = entity.Type,
                Name = entity.Name,
            };
        }

        public static explicit operator ImporterType(Models.ImporterType model)
        {
            return new ImporterType
            {
                ImporterTypeId = model.Id,
                Type = model.Type,
                Name = model.Name,
            };
        }
    }
}
