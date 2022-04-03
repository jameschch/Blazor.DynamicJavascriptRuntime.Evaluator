window.JsInterop = {

    anonymous: null,

    set: function (value) {
        this.anonymous = value;
    },

    specified: null,

    setSpecified: function (value) {
        this.specified = value;
    },

    callMethod: function (value) {
        return value + 1;
    },

    returnValue: null,

    anotherReturnValue: null

};
