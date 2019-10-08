using System.Dynamic;
using System.Threading.Tasks;

namespace Blazor.DynamicJavascriptRuntime.Evaluator
{
    public interface IEvalContext
    {
        string this[string i] { set; }

        System.Func<dynamic> Expression { get; set; }

        void Dispose();
        ValueTask<T> InvokeAsync<T>();
        ValueTask<T> InvokeAsync<T>(string script);
        string ToString();
        bool TryCreateInstance(CreateInstanceBinder binder, object[] args, out object result);
        bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result);
        bool TryGetMember(GetMemberBinder binder, out object result);
        bool TryInvoke(InvokeBinder binder, object[] args, out object result);
        bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result);
        bool TrySetMember(SetMemberBinder binder, object value);
    }
}