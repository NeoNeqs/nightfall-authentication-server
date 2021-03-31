#ifndef MYSQL_H
#define MYSQL_H

#include <Godot.hpp>
#include <Object.hpp>
#include <FuncRef.hpp>
#include <Ref.hpp>
#include <Thread.hpp>
#include <Mutex.hpp>
#include <Semaphore.hpp>

#include <mysql_driver.h>
#include <cppconn/resultset.h>
#include <boost/smart_ptr.hpp>

#include <queue>
#include <memory>

namespace godot {

class MySQL : public Object {
    GODOT_CLASS(MySQL, Object);

private:
    sql::mysql::MySQL_Driver *driver;
    std::shared_ptr<sql::Connection> connection;
	sql::ConnectOptionsMap connection_properties;

    Thread *thread;
	Mutex *mutex;
	Semaphore *semaphore;

    enum Task {
		CONNECT_TO_DATABSE = 0,
		SET_SCHEMA = 1,
		EXECUTE_QUERY = 2,
		EXECUTE_PREPARED_QUERY = 3,
		EXECUTE_UPDATE_QUERY = 4,
		EXECUTE_PREPARED_UPDATE_QUERY = 5,
		EXECUTE_SELECT_QUERY = 6,
		EXECUTE_PREPARED_SELECT_QUERY = 7,
		FETCH_ARRAY = 8,
		FETCH_PREPARED_ARRAY = 9,
		FETCH_DICTIONARY = 10,
		FETCH_PREPARED_DICTIONARY = 11,
		CLOSE_CONNECTION = 12,
	};

    struct QueueItem {
		Task task;
		Array user_data;
	};

    std::queue<QueueItem> item_queue;

    bool exit;

	void _connect_to_database(Object *p_target, const String &p_callback, const Array &p_args);
	void _set_schema(const String &p_schema, Object *p_target, const String &p_callback, const Array &p_args);

	void _execute_query(const String &p_query, Object *p_target, const String &p_callback, const Array &p_args);
	void _execute_prepared_query(const String &p_query, const Array &p_params, Object *p_target, const String &p_callback, const Array &p_args);

	void _execute_update_query(const String &p_query, Object *p_target, const String &p_callback, const Array &p_args);
	void _execute_prepared_update_query(const String &p_query, const Array &p_params, Object *p_target, const String &p_callback, const Array &p_args);

	void _execute_select_query(const String &p_query, Object *p_target, const String &p_callback, const Array &p_args);
	void _execute_prepared_select_query(const String &p_query, const Array &p_params, Object *p_target, const String &p_callback, const Array &p_args);

	void _fetch_array(const String &p_query, Object *p_target, const String &p_callback, const Array &p_args);
	void _fetch_prepared_array(const String &p_query, const Array &p_params, Object *p_target, const String &p_callback, const Array &p_args);

	void _fetch_dictionary(const String &p_query, Object *p_target, const String &p_callback, const Array &p_args);
	void _fetch_prepared_dictionary(const String &p_query, const Array &p_params, Object *p_target, const String &p_callback, const Array &p_args);

	void _close_connection(Object *p_target, const String &p_callback, const Array &p_args);
	void _close_connection();

	bool _is_connected_to_database() const;

	static bool _is_sql_datetime(const String &p_datetime);

	static void _prepare_statement(const std::unique_ptr<sql::PreparedStatement> &p_prepared_statement, const Array &p_params);

	static void _process_result_set_as_dictionary(const std::unique_ptr<sql::ResultSet> &p_result_set, Array *p_result_array);
	static void _process_result_set_as_array(const std::unique_ptr<sql::ResultSet> &p_result_set, Array *p_result_array);

	template<class... Args>
	inline static void _call_function(Object *p_target, const String &p_callback, Args ...p_args) {
		if (p_target) {
			p_target->call(p_callback, p_args...);
		}
	}

	inline static sql::SQLString godot_string_to_sql(const String &p_string) {
		sql::SQLString sql_string(p_string.utf8().get_data());
		return sql_string;
	}

	inline static String sql_string_to_godot(const sql::SQLString &p_sql_string) {
		String string(p_sql_string.c_str());
		return string;
	}
	
	void _thread();

public:
    static void _register_methods();

    void _init();

	void thread_func(const Array &p_data);

    void connect_to_database(Object *p_target, const String &p_callback, const Array &p_args);
    void set_credentials(const String &p_host, const String &p_username, const String &p_password, int p_port);
	
	void set_schema(const String &p_schema, Object *p_target, const String &p_callback, const Array &p_args);

	void execute_query(const String &p_query, Object *p_target, const String &p_callback, const Array &p_args);
	void execute_prepared_query(const String &p_query, const Array &p_params, Object *p_target, const String &p_callback, const Array &p_args);

	void execute_update_query(const String &p_query, Object *p_target, const String &p_callback, const Array &p_args);
	void execute_prepared_update_query(const String &p_query, const Array &p_params, Object *p_target, const String &p_callback, const Array &p_args);

	void execute_select_query(const String &p_query, Object *p_target, const String &p_callback, const Array &p_args);
	void execute_prepared_select_query(const String &p_query, const Array &p_params, Object *p_target, const String &p_callback, const Array &p_args);

	void fetch_array(const String &p_query, Object *p_target, const String &p_callback, const Array &p_args);
	void fetch_prepared_array(const String &p_query, const Array &p_params, Object *p_target, const String &p_callback, const Array &p_args);

	void fetch_dictionary(const String &p_query, Object *p_target, const String &p_callback, const Array &p_args);
	void fetch_prepared_dictionary(const String &p_query, const Array &p_params, Object *p_target, const String &p_callback, const Array &p_args);

	void close_connection(Object *p_target, const String &p_callback, const Array &p_args);

    MySQL();
    ~MySQL();
};

}

#endif // MYSQL_H