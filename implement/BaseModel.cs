using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace klib.Implement
{
    /// <summary>
    /// Implement the Model class.
    /// </summary>
    public interface IBaseModel
    {
        
    }
    public abstract class BaseModel
    {
        protected virtual void Load(List<klib.Dynamic> values, BaseModel obj)
        {
            var p = obj.GetType();
            foreach (var proper in p.GetProperties(BindingFlags.Public))
            {
                var name = proper.Name.ToUpper();

                if (values.Where(c => c.Name == name).Any())
                {
                    var value = values.Where(c => c.Name == name).Select(c => c.Value).FirstOrDefault();
                    proper.SetValue(null, value);
                }

            }
        }
    }
}
