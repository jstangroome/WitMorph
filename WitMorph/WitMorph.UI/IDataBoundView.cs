namespace WitMorph.UI
{
    interface IDataBoundView<in TModel>
    {
        void SetDataSource(TModel model);
    }
}