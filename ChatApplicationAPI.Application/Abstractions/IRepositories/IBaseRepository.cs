﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ChatApplicationAPI.Application.Abstractions.IRepositories
{
    public interface IBaseRepository<T> where T : class
    {
        public Task<T> Create(T entity);
        public Task<T> GetByAny(Expression<Func<T, bool>> expression);
        public Task<IEnumerable<T>> GetByAll(Expression<Func<T, bool>> expression);
        public Task<IEnumerable<T>> GetAll();
        public Task<bool> Delete(Expression<Func<T, bool>> expression);
        public Task<bool> DeleteMany(Expression<Func<T, bool>> expression);
        public Task<T> Update(T entity);
    }
}
