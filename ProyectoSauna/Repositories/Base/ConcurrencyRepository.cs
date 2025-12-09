// Repositories/Base/ConcurrencyRepository.cs - Repository con control de concurrencia opcional
using Microsoft.EntityFrameworkCore;
using ProyectoSauna.Models;
using ProyectoSauna.Repositories.Interfaces;
using ProyectoSauna.Services;
using System.Linq.Expressions;

namespace ProyectoSauna.Repositories.Base
{
    /// <summary>
    /// Repository extendido con control de concurrencia OPCIONAL
    /// Hereda de Repository normal, así NO ROMPE nada existente
    /// </summary>
    public class ConcurrencyRepository<T> : Repository<T>, IConcurrencyRepository<T> where T : class
    {
        private readonly ConcurrencyService _concurrencyService;
        private readonly bool _useConcurrency;

        public ConcurrencyRepository(SaunaDbContext context, ConcurrencyService concurrencyService, bool useConcurrency = false) 
            : base(context)
        {
            _concurrencyService = concurrencyService;
            _useConcurrency = useConcurrency;
        }

        /// <summary>
        /// Update con control de concurrencia opcional
        /// Si falla, devuelve el resultado pero NO rompe
        /// </summary>
        public async Task<ConcurrencyResult> SafeUpdateAsync(T entity)
        {
            if (!_useConcurrency)
            {
                // Funcionamiento normal sin concurrencia
                _dbSet.Update(entity);
                await _context.SaveChangesAsync();
                return new ConcurrencyResult { Success = true, Message = "Actualizado exitosamente" };
            }

            // Con control de concurrencia
            _dbSet.Update(entity);
            return await _concurrencyService.SafeSaveChangesAsync();
        }

        /// <summary>
        /// Verifica si una entidad fue modificada externamente
        /// </summary>
        public async Task<bool> IsEntityModifiedExternallyAsync(T entity, params Expression<Func<T, object>>[] properties)
        {
            var currentValues = _context.Entry(entity).CurrentValues;
            var databaseValues = await _context.Entry(entity).GetDatabaseValuesAsync();

            if (databaseValues == null)
                return false; // Entidad fue eliminada

            foreach (var property in properties)
            {
                var propertyName = GetPropertyName(property);
                if (!currentValues[propertyName].Equals(databaseValues[propertyName]))
                    return true;
            }

            return false;
        }

        private string GetPropertyName<TProperty>(Expression<Func<T, TProperty>> property)
        {
            if (property.Body is MemberExpression member)
                return member.Member.Name;
            throw new ArgumentException("Expression must be a member expression");
        }
    }

    public interface IConcurrencyRepository<T> : IRepository<T> where T : class
    {
        Task<ConcurrencyResult> SafeUpdateAsync(T entity);
        Task<bool> IsEntityModifiedExternallyAsync(T entity, params Expression<Func<T, object>>[] properties);
    }
}