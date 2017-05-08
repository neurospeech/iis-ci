AtomConfig.ajax.jsonPostEncode = function (o) {
    o.contentType = 'application/json';
    o.data = JSON.stringify(o.data);
    return o;
};

